using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;

namespace MonoGame.Extended.BitmapFonts
{
    public static class BitmapFontExtensions
    {
        /// <summary>
        ///     Adds a string to a batch of sprites for rendering using the specified font, text, position, color, rotation,
        ///     origin, scale, effects and layer.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="bitmapFont">A font for displaying text.</param>
        /// <param name="text">The text message to display.</param>
        /// <param name="position">The location (in screen coordinates) to draw the text.</param>
        /// <param name="color">
        ///     The <see cref="Color" /> to tint a sprite. Use <see cref="Color.White" /> for full color with no
        ///     tinting.
        /// </param>
        /// <param name="rotation">Specifies the angle (in radians) to rotate the text about its origin.</param>
        /// <param name="origin">The origin for each letter; the default is (0,0) which represents the upper-left corner.</param>
        /// <param name="scale">Scale factor.</param>
        /// <param name="effect">Effects to apply.</param>
        /// <param name="layerDepth">
        ///     The depth of a layer. By default, 0 represents the front layer and 1 represents a back layer.
        ///     Use SpriteSortMode if you want sprites to be sorted during drawing.
        /// </param>
        public static void DrawString(this SpriteBatch spriteBatch, BitmapFont bitmapFont, string text, Vector2 position,
            Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float layerDepth)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (effect != SpriteEffects.None) throw new NotSupportedException($"{effect} is not currently supported for {nameof(BitmapFont)}");

            var dx = position.X;
            var dy = position.Y;

            for (var i = 0; i < text.Length; i++)
            {
                var character = BitmapFont.GetUnicodeCodePoint(text, i);
                var fontRegion = bitmapFont.GetCharacterRegion(character);

                if (fontRegion != null)
                {
                    var characterPosition = new Vector2(dx + fontRegion.XOffset, dy + fontRegion.YOffset);
                    var characterOrigin = position - characterPosition + origin;

                    spriteBatch.Draw(fontRegion.TextureRegion, position, color, rotation, characterOrigin, scale, effect, layerDepth);

                    dx += fontRegion.XAdvance + bitmapFont.LetterSpacing;
                }

                if (character == '\n')
                {
                    dy += bitmapFont.LineHeight;
                    dx = position.X;
                }
            }
        }

        /// <summary>
        ///     Adds a string to a batch of sprites for rendering using the specified font, text, position, color, rotation,
        ///     origin, scale, effects and layer.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="font">A font for displaying text.</param>
        /// <param name="text">The text message to display.</param>
        /// <param name="position">The location (in screen coordinates) to draw the text.</param>
        /// <param name="color">
        ///     The <see cref="Color" /> to tint a sprite. Use <see cref="Color.White" /> for full color with no
        ///     tinting.
        /// </param>
        /// <param name="rotation">Specifies the angle (in radians) to rotate the text about its origin.</param>
        /// <param name="origin">The origin for each letter; the default is (0,0) which represents the upper-left corner.</param>
        /// <param name="scale">Scale factor.</param>
        /// <param name="effect">Effects to apply.</param>
        /// <param name="layerDepth">
        ///     The depth of a layer. By default, 0 represents the front layer and 1 represents a back layer.
        ///     Use SpriteSortMode if you want sprites to be sorted during drawing.
        /// </param>
        public static void DrawString(this SpriteBatch spriteBatch, BitmapFont font, string text, Vector2 position,
            Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, float layerDepth)
        {
            DrawString(spriteBatch, font, text, position, color, rotation, origin, new Vector2(scale, scale), effect, layerDepth);
        }

        /// <summary>
        ///     Adds a string to a batch of sprites for rendering using the specified font, text, position, color, layer,
        ///     and width (in pixels) where to wrap the text at.
        /// </summary>
        /// <remarks>
        ///     <see cref="BitmapFont" /> objects are loaded from the Content Manager. See the <see cref="BitmapFont" /> class for
        ///     more information.
        ///     Before any calls to <see cref="DrawString" /> you must call <see cref="SpriteBatch.Begin" />. Once all calls to
        ///     <see cref="DrawString" /> are complete, call <see cref="SpriteBatch.End" />.
        ///     Use a newline character (\n) to draw more than one line of text.
        /// </remarks>
        /// <param name="spriteBatch"></param>
        /// <param name="font">A font for displaying text.</param>
        /// <param name="text">The text message to display.</param>
        /// <param name="position">The location (in screen coordinates) to draw the text.</param>
        /// <param name="color">
        ///     The <see cref="Color" /> to tint a sprite. Use <see cref="Color.White" /> for full color with no
        ///     tinting.
        /// </param>
        /// <param name="layerDepth">
        ///     The depth of a layer. By default, 0 represents the front layer and 1 represents a back layer.
        ///     Use SpriteSortMode if you want sprites to be sorted during drawing.
        /// </param>
        /// <param name="wrapWidth">The width (in pixels) where to wrap the text at.</param>
        public static void DrawString(this SpriteBatch spriteBatch, BitmapFont font, string text, Vector2 position,
            Color color, float layerDepth, int wrapWidth = int.MaxValue)
        {
            DrawWrapped(spriteBatch, font, text, position, color, wrapWidth, layerDepth);
        }

        /// <summary>
        ///     Adds a string to a batch of sprites for rendering using the specified font, text, position, color,
        ///     and width (in pixels) where to wrap the text at. The text is drawn on layer 0f.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="font">A font for displaying text.</param>
        /// <param name="text">The text message to display.</param>
        /// <param name="position">The location (in screen coordinates) to draw the text.</param>
        /// <param name="color">
        ///     The <see cref="Color" /> to tint a sprite. Use <see cref="Color.White" /> for full color with no
        ///     tinting.
        /// </param>
        /// <param name="wrapWidth">The width (in pixels) where to wrap the text at.</param>
        public static void DrawString(this SpriteBatch spriteBatch, BitmapFont font, string text, Vector2 position,
            Color color, int wrapWidth = int.MaxValue)
        {
            DrawWrapped(spriteBatch, font, text, position, color, wrapWidth, 0f);
        }

        /// <summary>
        ///     Method to handle wrapping text at a specified width. Passes onto the <see cref="DrawInto" /> method
        ///     if the user passes in int.MaxValue as the width.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="font">A font for displaying text.</param>
        /// <param name="text">The text message to display.</param>
        /// <param name="position">The location (in screen coordinates) to draw the text.</param>
        /// <param name="color">
        ///     The <see cref="Color" /> to tint a sprite. Use <see cref="Color.White" /> for full color with no
        ///     tinting.
        /// </param>
        /// <param name="wrapWidth">The width (in pixels) where to wrap the text at.</param>
        /// <param name="layerDepth">
        ///     The depth of a layer. By default, 0 represents the front layer and 1 represents a back layer.
        ///     Use SpriteSortMode if you want sprites to be sorted during drawing.
        /// </param>
        private static void DrawWrapped(this SpriteBatch spriteBatch, BitmapFont font, string text, Vector2 position,
            Color color, int wrapWidth, float layerDepth)
        {
            if (font == null) throw new ArgumentNullException(nameof(font));
            if (text == null) throw new ArgumentNullException(nameof(text));

            if (wrapWidth == int.MaxValue)
            {
                DrawString(spriteBatch, font, text, position, color, layerDepth);
                return;
            }

            // parse the text and wrap it at the specified width
            var dx = position.X;
            var dy = position.Y;
            var sentences = text.Split(new[] {'\n'}, StringSplitOptions.None);

            foreach (var sentence in sentences)
            {
                var words = sentence.Split(new[] {' '}, StringSplitOptions.None);

                for (var i = 0; i < words.Length; i++)
                {
                    var word = words[i];
                    var size = font.GetStringRectangle(word, Vector2.Zero);

                    if ((i != 0) && (dx + size.Width >= wrapWidth))
                    {
                        dy += font.LineHeight;
                        dx = position.X;
                    }

                    DrawString(spriteBatch, font, word, new Vector2(dx, dy), color, layerDepth);
                    dx += size.Width;

                    var spaceCharRegion = font.GetCharacterRegion(' ');
                    if (i != words.Length - 1)
                        dx += spaceCharRegion.XAdvance + font.LetterSpacing;
                    else
                        dx += spaceCharRegion.XOffset + spaceCharRegion.Width;
                }

                dx = position.X;
                dy += font.LineHeight;
            }
        }
    }
}