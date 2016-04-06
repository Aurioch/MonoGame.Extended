using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Gui.Controls
{
    public abstract class GuiContainerControl : GuiControl
    {
        protected GuiContainerControl()
        {
            HorizontalAlignment = GuiHorizontalAlignment.Stretch;
            VerticalAlignment = GuiVerticalAlignment.Stretch;
            Controls = new GuiControlCollection(this);
        }

        public GuiControlCollection Controls { get; }
        public GuiThickness Padding { get; set; }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            foreach (var control in Controls)
                control.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            foreach (var control in Controls)
                control.Draw(spriteBatch);
        }
    }
}