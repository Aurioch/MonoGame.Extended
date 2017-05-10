using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Screens
{
    public interface IScreenManager
    {
        T FindScreen<T>() where T : Screen;
    }

    public class ScreenManagerComponent : DrawableGameComponent, IScreenManager
    {
        /// <summary> A default SpriteBatch shared by all the screens. </summary>
        public SpriteBatch SpriteBatch { get; internal set; }

        /// <summary>
        /// If true, the manager prints out a list of all the screens
        /// each time it is updated. This can be useful for making sure
        /// everything is being added and removed at the right times.
        /// </summary>
        public bool TraceEnabled { get; internal set; }

        private List<Screen> _screens;
        private List<Screen> _tempScreenList;
        private bool _isInitialized;
        private Texture2D _blankTexture;

        public ScreenManagerComponent(Game game) : base(game)
        {
            _screens = new List<Screen>();
            _tempScreenList = new List<Screen>();
            _isInitialized = false;
        }

        public T FindScreen<T>() where T : Screen
        {
            var screen = _screens.OfType<T>().FirstOrDefault();

            if (screen == null)
                throw new InvalidOperationException($"{typeof(T).Name} not added");

            return screen;
        }

        public override void Initialize()
        {
            base.Initialize();

            _isInitialized = true;
        }
        
        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Set blank texture, used for shading of screen during transition
            const int blankSize = 8;
            _blankTexture = new Texture2D(GraphicsDevice, blankSize, blankSize);
            var color = new Color[blankSize * blankSize];
            for (int i = 0; i < color.Length; i++)
                color[i] = Color.White;
            _blankTexture.SetData(color);

            // Each screen loads content
            foreach (var screen in _screens)
                screen.Activate();
        }

        protected override void UnloadContent()
        {
            foreach (var screen in _screens)
                screen.Unload();
        }

        /// <summary> Allows each screen to run logic. </summary>
        public override void Update(GameTime gameTime)
        {
            // Makes a temp copy of master screen list for editing
            _tempScreenList.Clear();
            foreach (var screen in _screens)
                _tempScreenList.Add(screen);

            bool otherScreenHasFocus = !Game.IsActive;
            bool coveredByOtherScreen = false;

            // Loop as long as there are screens waiting to be updated.
            while (_tempScreenList.Count > 0)
            {
                // Pop the topmost screen off the waiting list.
                var screen = _tempScreenList[_tempScreenList.Count - 1];

                _tempScreenList.RemoveAt(_tempScreenList.Count - 1);

                if (!screen.Enabled)
                    continue;

                // Update the screen.
                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.ScreenState == ScreenState.TransitionOn ||
                    screen.ScreenState == ScreenState.Active)
                {
                    // If this is the first active screen we came across,
                    // give it a chance to handle input.
                    if (!otherScreenHasFocus)
                    {
                        screen.HandleInput(gameTime);

                        otherScreenHasFocus = true;
                    }

                    // If this is an active non-popup, inform any subsequent
                    // screens that they are covered by it.
                    if (!screen.IsPopup)
                        coveredByOtherScreen = true;
                }
            }

            // Print debug trace?
            if (TraceEnabled)
                TraceScreens();
        }


        /// <summary> Prints a list of all the screens, for debugging. </summary>
        void TraceScreens()
        {
            List<string> screenNames = new List<string>();

            foreach (var screen in _screens)
                screenNames.Add(screen.GetType().Name);

            System.Diagnostics.Debug.WriteLine(string.Join(", ", screenNames.ToArray()));
        }

        /// <summary> Tells each screen to draw itself. </summary>
        public override void Draw(GameTime gameTime)
        {
            foreach (var screen in _screens)
            {
                if (screen.ScreenState == ScreenState.Hidden)
                    continue;

                screen.Draw(gameTime);
            }
        }

        /// <summary> Add new screen to the screen manager. </summary>
        public void AddScreen(Screen screen)
        {
            screen.ScreenManager = this;

            // If we have a graphics device, tell the screen to load content.
            if (_isInitialized)
                screen.Activate();

            _screens.Add(screen);
        }

        /// <summary>
        /// Removes a screen from the screen manager. You should normally
        /// use GameScreen.ExitScreen instead of calling this directly, so
        /// the screen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        public void RemoveScreen(Screen screen)
        {
            // If we have a graphics device, tell the screen to unload content.
            if (_isInitialized)
                screen.Unload();
            
            _screens.Remove(screen);
            _tempScreenList.Remove(screen);
        }

        /// <summary>
        /// Helper draws a translucent black fullscreen sprite, used for fading
        /// screens in and out, and for darkening the background behind popups.
        /// </summary>
        public void FadeBackBufferToBlack(float alpha)
        {
            SpriteBatch.Begin();
            SpriteBatch.Draw(_blankTexture, GraphicsDevice.Viewport.Bounds, Color.Black * alpha);
            SpriteBatch.End();
        }
    }
}