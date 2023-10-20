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

namespace SatelliteStorage.UIElements
{
	class TextBox : UIPanel
	{
		internal bool focused = false;

		private int _maxLength = 60;

		public string hintText = "";
		public string currentString = "";
		private int textBlinkerCount;
		private int textBlinkerState;

		public event Action OnFocus;
		public event Action OnUnfocus;
		public event Action OnTextChanged;
		public event Action OnTabPressed;
		public event Action OnEnterPressed;

		internal bool unfocusOnEnter = true;

		internal bool unfocusOnTab = true;

		public Color textColor = Color.Black;
		public float textScale = 1;
		public Vector2 textPosition = new Vector2(4, 2);
		public int visibleTextCount = 10;

		public TextBox()
		{
			SetPadding(0);
			BackgroundColor = Color.White;
			BorderColor = Color.White;
		}


        public override void LeftClick(UIMouseEvent evt)
		{
			Focus();
		}

		public override void RightClick(UIMouseEvent evt)
		{
			base.RightClick(evt);
			SetText("");
		}

		public void SetUnfocusKeys(bool unfocusOnEnter, bool unfocusOnTab)
		{
			this.unfocusOnEnter = unfocusOnEnter;
			this.unfocusOnTab = unfocusOnTab;
		}

		public void Unfocus()
		{
			if (focused)
			{
				focused = false;
				Main.blockInput = false;

				OnUnfocus?.Invoke();
			}
		}

		public void Focus()
		{
			if (!focused)
			{
				Main.clrInput();
				focused = true;
				Main.blockInput = true;

				OnFocus?.Invoke();
			}
		}

		public override void Update(GameTime gameTime)
		{
			Vector2 MousePosition = new Vector2((float)Main.mouseX, (float)Main.mouseY);
			if (!ContainsPoint(MousePosition) && (Main.mouseLeft || Main.mouseRight))
			{
				Unfocus();
			}
			base.Update(gameTime);
		}

		public void SetText(string text)
		{
			if (text.ToString().Length > this._maxLength)
			{
				text = text.ToString().Substring(0, this._maxLength);
			}
			if (currentString != text)
			{
				currentString = text;
				OnTextChanged?.Invoke();
			}
		}

		public void SetTextMaxLength(int maxLength)
		{
			this._maxLength = maxLength;
		}

		private static bool JustPressed(Keys key)
		{
			return Main.inputText.IsKeyDown(key) && !Main.oldInputText.IsKeyDown(key);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			Rectangle hitbox = GetInnerDimensions().ToRectangle();
			base.DrawSelf(spriteBatch);

			if (focused)
			{
				Terraria.GameInput.PlayerInput.WritingText = true;
				Main.instance.HandleIME();
				string newString = Main.GetInputText(currentString);
				if (!newString.Equals(currentString))
				{
					currentString = newString;
					OnTextChanged?.Invoke();
				}
				else
				{
					currentString = newString;
				}

				if (JustPressed(Keys.Tab))
				{
					if (unfocusOnTab) Unfocus();
					OnTabPressed?.Invoke();
				}
				if (JustPressed(Keys.Enter))
				{
					Main.drawingPlayerChat = false;
					if (unfocusOnEnter) Unfocus();
					OnEnterPressed?.Invoke();
				}
				if (++textBlinkerCount >= 20)
				{
					textBlinkerState = (textBlinkerState + 1) % 2;
					textBlinkerCount = 0;
				}
				Main.instance.DrawWindowsIMEPanel(new Vector2(98f, (float)(Main.screenHeight - 36)), 0f);
			}
			string displayString = currentString;
			CalculatedStyle space = base.GetDimensions();
			Color color = textColor;
			if (currentString.Length == 0)
			{
			}
			Vector2 drawPos = space.Position() + textPosition;
			if (currentString.Length == 0 && !focused)
			{
				color *= 0.5f;
				spriteBatch.DrawString(FontAssets.MouseText.Value, hintText, drawPos, color, 0, new Vector2(0, 0), textScale, SpriteEffects.None, 0);

			}
			else
			{

				string displayValue = displayString;

				
				if (displayValue.Length > visibleTextCount+1)
                {
					int substFrom = displayString.Length - visibleTextCount - 1;
					if (substFrom <= 0) substFrom = 0;
					int substCount = visibleTextCount;
					if (substCount <= 0) substCount = 0;
					SatelliteStorage.Debug(substFrom + " - " + substCount + " : "+ displayString.Length);

					displayValue = displayValue.Substring(substFrom, substCount);
				}

				if (this.textBlinkerState == 1 && focused)
				{
					displayValue = displayValue + "|";
				}
				
				spriteBatch.DrawString(FontAssets.MouseText.Value, displayValue, drawPos, color, 0, new Vector2(0,0), textScale, SpriteEffects.None, 0);
			}
		}
	}
}