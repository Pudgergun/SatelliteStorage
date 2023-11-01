using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using System;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.Audio;
using Terraria.ID;

namespace SatelliteStorage.UI
{
    class TextButton : UIPanel
    {
        public Vector2 textPosition;
        public string text;
        public float textScale;
        private float scaleOffset = 0;
        private UIText textElement;
        private bool mouseOver = false;

        public TextButton(string text)
        {
            this.text = text;
            textPosition = new Vector2(0, 0);
            textScale = 1;

            Width.Set(MathF.Round(100 * text.Length / 10), 0);
            Height.Set(MathF.Round(30 * text.Length / 10), 0);

            BackgroundColor = new Color(0, 0, 0, 0);
            BorderColor = new Color(0, 0, 0, 0);

            OnMouseOver += (evt, listeningElement) =>
            {
                if (mouseOver) return;
                SoundEngine.PlaySound(SoundID.MenuTick);
                mouseOver = true;
            };

            OnMouseOut += (evt, listeningElement) =>
            {
                if (!mouseOver) return;
                mouseOver = false;
            };
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Color color = Color.White;
            if (mouseOver)
            {
                if (scaleOffset < textScale * 0.2f) scaleOffset += textScale * 0.05f;
                color = new Color(255, 231, 69, 255);
            }
            else
            {
                if (scaleOffset > 0) scaleOffset -= textScale * 0.05f;
            }

            if (textElement != null)
            {
                textElement.Deactivate();
                RemoveChild(textElement);
            }

            textElement = new UIText(text, textScale + scaleOffset);
            textElement.VAlign = 0.5f;
            textElement.Left.Set(0, 0);
            textElement.TextColor = color;
            textElement.TextOriginX = 0;


            Append(textElement);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            base.Draw(spriteBatch);
        }
    }
}
