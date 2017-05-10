using System;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Screens
{
    /// <summary> Enum describes the screen transition state. </summary>
    public enum ScreenState
    {
        TransitionOn,
        Active,
        TransitionOff,
        Hidden,
    }

    /// <summary>
    /// A screen is a single layer that has update and draw logic, and which
    /// can be combined with other layers to build up a complex menu system.
    /// For instance the main menu, the options menu, the "are you sure you
    /// want to quit" message box, and the main game itself are all implemented
    /// as screens.
    /// </summary>
    public abstract class Screen : IDisposable
    {
        /// <summary>
        /// Normally when one screen is brought up over the top of another,
        /// the first screen will transition off to make room for the new
        /// one. This property indicates whether the screen is only a small
        /// popup, in which case screens underneath it do not need to bother
        /// transitioning off.
        /// </summary>
        public bool IsPopup { get; set; }

        /// <summary>
        /// Indicates whether the screen should update itself or not.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Indicates how long the screen takes to
        /// transition on when it is activated.
        /// </summary>
        public TimeSpan TransitionOnTime { get; protected set; }

        /// <summary>
        /// Indicates how long the screen takes to
        /// transition off when it is deactivated.
        /// </summary>
        public TimeSpan TransitionOffTime { get; protected set; }

        /// <summary>
        /// Gets the current position of the screen transition, ranging
        /// from zero (fully active, no transition) to one (transitioned
        /// fully off to nothing).
        /// </summary>
        public float TransitionPosition { get; protected set; }

        /// <summary>
        /// Gets the current alpha of the screen transition, ranging
        /// from 1 (fully active, no transition) to 0 (transitioned
        /// fully off to nothing).
        /// </summary>
        public float TransitionAlpha { get { return 1f - TransitionPosition; } }

        /// <summary> Gets the current screen transition state. </summary>
        public ScreenState ScreenState { get; protected set; }

        /// <summary>
        /// There are two possible reasons why a screen might be transitioning
        /// off. It could be temporarily going away to make room for another
        /// screen that is on top of it, or it could be going away for good.
        /// This property indicates whether the screen is exiting for real:
        /// if set, the screen will automatically remove itself as soon as the
        /// transition finishes.
        /// </summary>
        public bool IsExiting { get; protected set; }

        /// <summary> Checks whether this screen is active and can respond to user input. </summary>
        public bool IsActive
        {
            get { return !_otherScreenHasFocus && (ScreenState == ScreenState.TransitionOn || ScreenState == ScreenState.Active); }
        }

        /// <summary> Gets the manager that this screen belongs to. </summary>
        public ScreenManagerComponent ScreenManager { get; set; }

        private bool _otherScreenHasFocus;

        public Screen()
        {
            IsPopup = false;
            TransitionOnTime = TimeSpan.Zero;
            TransitionOffTime = TimeSpan.Zero;
            TransitionPosition = 1f;
            ScreenState = ScreenState.TransitionOn;
            IsExiting = false;
            Enabled = true;
        }

        public virtual void Dispose() { }

        /// <summary>
        /// Activates the screen. Called when the screen is added to the screen manager or if the game resumes
        /// from being paused or tombstoned.
        /// </summary>
        /// <param name="instancePreserved">
        /// True if the game was preserved during deactivation, false if the screen is just being added or if the game was tombstoned.
        /// On desktop it will always be false, on which the parameter can be omitted.
        /// </param>
        public virtual void Activate(bool instancePreserver = false) { }

        /// <summary> Deactivates the screen. Called when the game is being deactivated due to pausing or tombstoning. </summary>
        public virtual void Deactivate() { }

        /// <summary> Unload content for the screen.Called when the screen is removed from the screen manager. </summary>
        public virtual void Unload() { }

        /// <summary>
        /// Allows the screen to run logic, such as updating the transition position.
        /// Unlike HandleInput, this method is called regardless of whether the screen
        /// is active, hidden, or in the middle of a transition.
        /// </summary>
        public virtual void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            _otherScreenHasFocus = otherScreenHasFocus;

            if (IsExiting)
            {
                // If the screen is going away to die, it should transition off.
                ScreenState = ScreenState.TransitionOff;

                if (!UpdateTransition(gameTime, TransitionOffTime, 1))
                {
                    // When the transition finishes, remove the screen.
                    ScreenManager.RemoveScreen(this);
                }
            }
            else if (coveredByOtherScreen)
            {
                // If the screen is covered by another, it should transition off.
                if (UpdateTransition(gameTime, TransitionOffTime, 1))
                {
                    // Still busy transitioning.
                    ScreenState = ScreenState.TransitionOff;
                }
                else
                {
                    // Transition finished!
                    ScreenState = ScreenState.Hidden;
                }
            }
            else
            {
                // Otherwise the screen should transition on and become active.
                if (UpdateTransition(gameTime, TransitionOnTime, -1))
                {
                    // Still busy transitioning.
                    ScreenState = ScreenState.TransitionOn;
                }
                else
                {
                    // Transition finished!
                    ScreenState = ScreenState.Active;
                }
            }
        }

        /// <summary> Helper for updating the screen transition position. </summary>
        bool UpdateTransition(GameTime gameTime, TimeSpan time, int direction)
        {
            bool result = true;

            // How much should we move by?
            float transitionDelta = 1f;

            if (time != TimeSpan.Zero)
                transitionDelta = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / time.TotalMilliseconds);

            // Update the transition position.
            TransitionPosition += transitionDelta * direction;

            // Did we reach the end of the transition?
            if (((direction < 0) && (TransitionPosition <= 0)) ||
                ((direction > 0) && (TransitionPosition >= 1)))
            {
                TransitionPosition = MathHelper.Clamp(TransitionPosition, 0, 1);
                result = false;
            }

            // Otherwise we are still busy transitioning.
            return result;
        }

        /// <summary>
        /// Allows the screen to handle user input, in case InputListeners aren't used.
        /// Unlike Update, this method is only called when the screen is active, 
        /// and not when some other screen has taken the focus.
        /// </summary>
        public virtual void HandleInput(GameTime gameTime) { }

        public virtual void Draw(GameTime gameTime) { }

        /// <summary>
        /// Tells the screen to go away. Unlike ScreenManager.RemoveScreen, which
        /// instantly kills the screen, this method respects the transition timings
        /// and will give the screen a chance to gradually transition off.
        /// </summary>
        public void ExitScreen()
        {
            if (TransitionOffTime == TimeSpan.Zero)
            {
                // If the screen has a zero transition time, remove it immediately.
                ScreenManager.RemoveScreen(this);
            }
            else
            {
                // Otherwise flag that it should transition off and then exit.
                IsExiting = true;
            }
        }
    }
}