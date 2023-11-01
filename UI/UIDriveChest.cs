using Terraria;
using Terraria.UI;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using SatelliteStorage.DriveSystem;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.UI.Chat;
using Terraria.UI.Gamepad;
using Terraria.GameInput;

namespace SatelliteStorage.UI
{
    public partial class UIDriveChest : UIBaseState
	{
		public bool mouseOver;
		private UserInterface _driveChestInterface;
		private GameTime _lastUpdateUiGameTime;

        private UIItemsDisplay display;
        private UICraftDisplay craftDisplay;
        private UICraftRecipeDisplay craftRecipeDisplay;
        private UICraftResultDisplay craftResultDisplay;

		private float windowWidth;
		private float windowHeight;

		private Vector2 buttonsPos;
		private Vector2 craftResultSlotPos;

		private Item craftResultItem = new Item(5, 5);

		private int craftOnMouseRecipe = -1;

		public class ButtonID
		{
			public const int DepositAll = 0;
			public const int QuickStack = 1;
		}

		public const float buttonScaleMinimum = 0.75f;
		public const float buttonScaleMaximum = 1f;
		public float[] ButtonScale = new float[2];
		public bool[] ButtonHovered = new bool[2];

		private Item itemOnMouse;

		public bool isDrawing = false;

		public int currentRecipe = -1;

		public bool isMouseDownOnCraftItem = false;
		private bool mouseDownOnCraftItemToggle = false;
		private bool isRightMouseDownOnDisplay = false;

		private IMouseDownHoldingInterval _mouseDownOnCraftItemInterval;
        private IMouseDownHoldingInterval _mouseDownOnDisplayItemInterval;

        public event Action<int> OnCraftItem;
        public event Action<bool> OnDepositItems;
        public event Action<bool> OnCheckRecipesRefreshState;
		public event Action OnItemsDisplayLeftMouseDown;
		public event Action OnRequestItems;
		public event Action<IDriveItem, int> OnDisplayItemClicked;
        public event Action<IDriveItem> OnDisplayItemToggleAutoImport;
		public event Action OnRequestAvailableRecipes;

        public void UpdateHover(int ID, bool hovering)
		{
            if (hovering)
			{
				if (!ButtonHovered[ID])
				{
					SoundEngine.PlaySound(SoundID.MenuTick);
				}
				ButtonHovered[ID] = true;
				ButtonScale[ID] += 0.05f;
				if (ButtonScale[ID] > 1f)
				{
					ButtonScale[ID] = 1f;
				}
			}
			else
			{
				ButtonHovered[ID] = false;
				ButtonScale[ID] -= 0.05f;
				if (ButtonScale[ID] < 0.75f)
				{
					ButtonScale[ID] = 0.75f;
				}
			}
		}

		public override void OnActivate()
        {
            base.OnActivate();
		}

		public void ReloadItems()
        {
			if (!GetState()) return;

			display.RebuildPage();
		}

		public void RebuildCraftingPages()
		{
            craftDisplay.RebuildPage();
            craftRecipeDisplay.RebuildPage();
        }


		private void RequestItems() => OnRequestItems.Invoke();
		
		public void UpdateItems(List<IDriveItem> items)
		{
			display?.UpdateItems(items);
        }

		public void UpdateAutoImportItems(Dictionary<int, IDriveItem> autoImportItems)
		{
			display?.UpdateAutoImportItems(autoImportItems);
		}

		public void UpdateAvailableRecipes(Dictionary<int, Recipe> recipes)
		{
			craftDisplay?.UpdateAvailableRecipes(recipes);
		}

		public override void OnInitialize()
		{
			base.OnInitialize();

			CalculateSize();

			_mouseDownOnCraftItemInterval = new MouseDownHoldingInterval();
            _mouseDownOnDisplayItemInterval = new MouseDownHoldingInterval();

            _driveChestInterface = new UserInterface();

			display = new UIItemsDisplay(this);
            
            craftRecipeDisplay = new UICraftRecipeDisplay(this);
            craftResultDisplay = new UICraftResultDisplay();

            craftDisplay = new UICraftDisplay(this, craftRecipeDisplay);

            Append(display);

			display.onFiltersChanged = () =>
			{
				craftDisplay._filterer = display._filterer;
				craftDisplay.UpdateContents();
			};

			display.OnLeftMouseDown += (evt, element) =>
			{
                OnItemsDisplayLeftMouseDown?.Invoke();
			};



			display.OnRightMouseDown += (evt, element) =>
                isRightMouseDownOnDisplay = true;

            display.OnRightMouseUp += (evt, element) =>
                isRightMouseDownOnDisplay = false;

            display.OnRequestItems += () => RequestItems();

			display.OnItemClicked += (item, clickType) => 
				OnDisplayItemClicked?.Invoke(item, clickType);

            display.OnToggleItemAutoImport += (item) => 
				OnDisplayItemToggleAutoImport?.Invoke(item);
			

            

			Append(craftDisplay);

			craftDisplay.OnRequestAvailableRecipes += () => 
				OnRequestAvailableRecipes?.Invoke();
			Append(craftRecipeDisplay);
			
			Append(craftResultDisplay);

			CalculateSize();

			craftDisplay.onRecipeChoosen += (int recipe) =>
			{
				currentRecipe = recipe;
				craftRecipeDisplay.SetRecipe(recipe);
				SoundEngine.PlaySound(SoundID.MenuTick);
			};
			
			OnLeftMouseDown += (UIMouseEvent evt, UIElement listeningElement) =>
			{
				if (craftOnMouseRecipe > -1)
                {
					mouseDownOnCraftItemToggle = false;
					isMouseDownOnCraftItem = true;
					CraftItem();
                }
			};

			OnLeftMouseUp += (UIMouseEvent evt, UIElement listeningElement) =>
			{
				isMouseDownOnCraftItem = false;
            };
        }

		private void _RightMouseDownOnItem()
		{
            if (display.hoveredItemIndex <= -1) return;
			if (display.driveItems == null) return;
            IDriveItem driveItem = display.driveItems[display.hoveredItemIndex];
            if (driveItem == null) return;
            OnDisplayItemClicked?.Invoke(driveItem, 1);
        }

		private void CraftItem()
		{
            if (craftOnMouseRecipe <= -1) return;
			OnCraftItem.Invoke(craftOnMouseRecipe);
        }

		private void DepositItems(bool newItems = false)
        {
			OnDepositItems.Invoke(newItems);
        }

		private void CheckRecipesRefreshState(bool state)
		{
			OnCheckRecipesRefreshState.Invoke(state);
        }

		private Rectangle GetSlotHitbox(int startX, int startY)
		{
			return new Rectangle(startX, startY, 44, 44);
		}

		public override void Draw(SpriteBatch spriteBatch)
        {
			craftOnMouseRecipe = -1;
			if (!isDrawing) return;

			if (Main.CreativeMenu.Enabled) Main.CreativeMenu.CloseMenu();
			if (Main.editChest) Main.editChest = false;

			Main.LocalPlayer.chest = -1;
			
			if (Main.npcChatText.Length > 0) Main.CloseNPCChatOrSign();

			if (!Main.hidePlayerCraftingMenu) Main.hidePlayerCraftingMenu = true;

			base.Draw(spriteBatch);


			Main.inventoryScale = 0.755f;
			if (Terraria.Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, 73f, Main.instance.invBottom, 560f * Main.inventoryScale, 224f * Main.inventoryScale))
			{
				Main.player[Main.myPlayer].mouseInterface = true;
			}
			DrawButtons(spriteBatch);

			if (craftDisplay != null && !craftDisplay.hidden)
            {
				Terraria.Utils.DrawBorderString(spriteBatch, Language.GetTextValue("Mods.SatelliteStorage.UITitles.DriveChestRecipes"), craftDisplay.GetDimensions().Position() + new Vector2(30, 47), Color.White, 1f);
			}

			if (craftRecipeDisplay != null && !craftRecipeDisplay.hidden)
			{
				Terraria.Utils.DrawBorderString(spriteBatch, Language.GetTextValue("Mods.SatelliteStorage.UITitles.DriveChestCraft"), craftRecipeDisplay.GetDimensions().Position() + new Vector2(30, 47), Color.White, 1f);
			}

			if (currentRecipe > -1 && !craftRecipeDisplay.hidden)
			{
				Recipe recipe = Main.recipe[currentRecipe];
				craftResultItem = recipe.createItem;
				Rectangle itemSlotHitbox = GetSlotHitbox((int)craftResultSlotPos.X, (int)craftResultSlotPos.Y);
				bool cREATIVE_ItemSlotShouldHighlightAsSelected = false;
				if (IsMouseHovering && itemSlotHitbox.Contains(Main.MouseScreen.ToPoint()) && !PlayerInput.IgnoreMouseInterface)
				{
					Main.LocalPlayer.mouseInterface = true;

					ItemSlot.OverrideHover(ref craftResultItem, 26);
					ItemSlot.MouseHover(ref craftResultItem, 26);

					craftOnMouseRecipe = currentRecipe;
					cREATIVE_ItemSlotShouldHighlightAsSelected = true;
				} else
                {
					isMouseDownOnCraftItem = false;
				}
				UILinkPointNavigator.Shortcuts.CREATIVE_ItemSlotShouldHighlightAsSelected = cREATIVE_ItemSlotShouldHighlightAsSelected;
				ItemSlot.Draw(spriteBatch, ref craftResultItem, 26, itemSlotHitbox.TopLeft());
				craftResultDisplay.hidden = false;
			} else
            {
                craftResultDisplay.hidden = true;
			}
			
		}

        public override void OnUpdateUI(GameTime gameTime)
		{

			isDrawing = false;

			if (_driveChestInterface?.CurrentState != null)
			{
				isDrawing = true;
				
				_lastUpdateUiGameTime = gameTime;

				if (isMouseDownOnCraftItem)
                {
					if (!mouseDownOnCraftItemToggle)
					{
						_mouseDownOnCraftItemInterval.Reset(gameTime.TotalGameTime.TotalMilliseconds);
                        mouseDownOnCraftItemToggle = true;
					}
				} else
                {
					mouseDownOnCraftItemToggle = false;
				}

				if (mouseDownOnCraftItemToggle && isMouseDownOnCraftItem)
                {
                    if (_mouseDownOnCraftItemInterval.CheckIsReady(gameTime.TotalGameTime.TotalMilliseconds))
						CraftItem();
				}

				if (isRightMouseDownOnDisplay && display?.hoveredItemIndex > -1)
				{
					if (_mouseDownOnDisplayItemInterval.CheckIsReady(gameTime.TotalGameTime.TotalMilliseconds))
						_RightMouseDownOnItem();
                }

				if (!isRightMouseDownOnDisplay)
					_mouseDownOnDisplayItemInterval.Reset(gameTime.TotalGameTime.TotalMilliseconds);


                if (Main.recBigList) isDrawing = false;

				CalculateSize();
				_driveChestInterface.Update(gameTime);

				Player player = Main.LocalPlayer;
				Item mouseItem = player.inventory[58];

				if (mouseItem.IsAir || Main.mouseItem.IsAir)
				{
					if (itemOnMouse != null)
					{
						itemOnMouse = null;
                        CheckRecipesRefreshState(false);
					}
				}
				else
				{

					if (itemOnMouse == null || itemOnMouse.IsNotSameTypePrefixAndStack(mouseItem))
					{
						itemOnMouse = mouseItem;
                        CheckRecipesRefreshState(false);
					}
				}
				
			}
		}

		private void CalculateSize()
        {
			float width = GetDimensions().Width;
			float height = GetDimensions().Height;

			
			if (windowHeight != height || windowWidth != width)
            {
				windowWidth = width;
				windowHeight = height;
				
				if (display != null)
				{
					display.HAlign = 0.5f;
					display.VAlign = 0.5f;

					display.Width.Set(width * 0.3f, 0);
					display.Height.Set(height * 0.6f, 0);
					display.MinWidth.Set(500, 0);


					bool smallWidth = false;
					if (width < 1650)
                    {
						display.Height.Set(height * 0.35f, 0);
						display.VAlign = 0.85f;
						smallWidth = true;
					}

					display.Recalculate();
					

					if (smallWidth)
					{
						buttonsPos = new Vector2(
							display.GetDimensions().X + 30,
							display.GetDimensions().Y - 50
						);
					}
					else
					{
						buttonsPos = new Vector2(
							display.GetDimensions().X + display.GetDimensions().Width + 15,
							display.GetDimensions().Y + 70
						);
					}
					

					float craftDisplayWidth = MathF.Max(width * 0.2f, 150);
					float craftDisplayHeight = MathF.Max(display.Height.Pixels * 0.77f, 150);
					craftDisplay.Left.Set(display.GetDimensions().X - craftDisplayWidth - 15, 0);
					craftDisplay.Top.Set(display.GetDimensions().Y + (display.Height.Pixels - craftDisplayHeight), 0);
					craftDisplay.Width.Set(craftDisplayWidth, 0);
					craftDisplay.Height.Set(craftDisplayHeight, 0);

					craftDisplay.Recalculate();

					if (smallWidth)
					{
						float craftRecipeWidth = MathF.Max(width * 0.1f, 150);
						float craftRecipeHeight = MathF.Max(craftDisplay.Height.Pixels * 0.55f, 150);
						craftRecipeDisplay.Left.Set(craftDisplay.GetDimensions().X, 0);
						craftRecipeDisplay.Top.Set(craftDisplay.GetDimensions().Y - craftRecipeHeight + 25, 0);
						craftRecipeDisplay.Width.Set(craftDisplay.Width.Pixels, 0);
						craftRecipeDisplay.Height.Set(craftRecipeHeight, 0);

						craftRecipeDisplay.Recalculate();

						float craftResultWidth = craftRecipeDisplay.Width.Pixels * 0.25f;
						float craftResultHeight = craftRecipeDisplay.Width.Pixels * 0.25f;
						craftResultDisplay.Left.Set(craftRecipeDisplay.GetDimensions().X + (craftRecipeDisplay.Width.Pixels / 2) - (craftResultWidth / 2), 0);
						craftResultDisplay.Top.Set(craftRecipeDisplay.GetDimensions().Y - craftResultHeight + 30, 0);
						craftResultDisplay.Width.Set(craftResultWidth, 0);
						craftResultDisplay.Height.Set(craftResultHeight, 0);
					} else
                    {
						float craftRecipeWidth = MathF.Max(width * 0.1f, 150);
						float craftRecipeHeight = MathF.Max(craftDisplay.Height.Pixels * 0.55f, 150);
						craftRecipeDisplay.Left.Set(craftDisplay.GetDimensions().X - craftRecipeWidth - 15, 0);
						craftRecipeDisplay.Top.Set(craftDisplay.GetDimensions().Y, 0);
						craftRecipeDisplay.Width.Set(craftRecipeWidth, 0);
						craftRecipeDisplay.Height.Set(craftRecipeHeight, 0);

						craftRecipeDisplay.Recalculate();


						float craftResultWidth = craftRecipeDisplay.Width.Pixels * 0.35f;
						craftResultDisplay.Left.Set(craftRecipeDisplay.GetDimensions().X + (craftRecipeDisplay.Width.Pixels / 2) - (craftResultWidth / 2), 0);
						craftResultDisplay.Top.Set(craftRecipeDisplay.GetDimensions().Y + craftRecipeDisplay.Height.Pixels + 15, 0);
						craftResultDisplay.Width.Set(craftRecipeDisplay.Width.Pixels * 0.35f, 0);
						craftResultDisplay.Height.Set(craftRecipeDisplay.Width.Pixels * 0.35f, 0);
					}



					craftResultDisplay.Recalculate();

					if (smallWidth)
                    {
						craftResultSlotPos = new Vector2(
							craftResultDisplay.GetDimensions().X + (craftResultDisplay.Width.Pixels / 2) - (40 / 2) - 1,
							craftResultDisplay.GetDimensions().Y + (craftResultDisplay.Height.Pixels / 2) - (40 / 2) + 1
						);
					} else
                    {
						craftResultSlotPos = new Vector2(
							craftResultDisplay.GetDimensions().X + (craftResultDisplay.Width.Pixels / 2) - (40 / 2),
							craftResultDisplay.GetDimensions().Y + (craftResultDisplay.Height.Pixels / 2) - (40 / 2)
						);
					}
				}
			}
		}

        public override void OnModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{


			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (mouseTextIndex != -1)
			{
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
					"SatelliteStorage: DriveChestUI",
					delegate
					{
						if (_lastUpdateUiGameTime != null && _driveChestInterface?.CurrentState != null)
						{
							_driveChestInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
						}
						return true;
					},
					   InterfaceScaleType.UI));
			}
		}

		public override void SetState(bool state)
        {
			if (state)
			{
				_driveChestInterface?.SetState(this);
				CalculateSize();
				ReloadItems();
                CheckRecipesRefreshState(false);
            }
			else
			{
				isMouseDownOnCraftItem = false;
				_driveChestInterface?.SetState(null);

				for (int i = 0; i < 2; i++)
				{
					ButtonScale[i] = 0.75f;
					ButtonHovered[i] = false;
				}

			}
		}

		public override bool GetState()
        {
			return _driveChestInterface?.CurrentState != null ? true : false;
		}

		private void DrawButtons(SpriteBatch spritebatch)
		{
			if (display == null) return;
			for (int i = 0; i < 2; i++)
			{
				DrawButton(spritebatch, i, (int)(buttonsPos.X), (int)(buttonsPos.Y));
			}
		}

		private void DrawButton(SpriteBatch spriteBatch, int ID, int X, int Y)
		{
			Player player = Main.player[Main.myPlayer];

			int num = ID;

			Y += num * 26;
			float num2 = ButtonScale[ID] * 1.3f;
			string text = "";
			switch (ID)
			{
				case 0:
					text = Lang.inter[30].Value;
					break;
				case 1:
					text = Lang.inter[31].Value;
					break;
			}
			Vector2 vector = FontAssets.MouseText.Value.MeasureString(text);
			Color color = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor) * num2;
			color = Color.White * 0.97f * (1f - (255f - (float)(int)Main.mouseTextColor) / 255f * 0.5f);
			color.A = byte.MaxValue;
			X += (int)(vector.X * (num2 / 2f));
			bool flag = Terraria.Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, (float)X - vector.X / 2f, Y - 12, vector.X, 24f);
			if (ButtonHovered[ID])
			{
				flag = Terraria.Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, (float)X - vector.X / 2f - 10f, Y - 12, vector.X + 16f, 24f);
			}
			if (flag)
			{
				color = Main.OurFavoriteColor;
			}
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, new Vector2(X, Y), color, 0f, vector / 2f, new Vector2(num2), -1f, 1.5f);
			vector *= num2;
			switch (ID)
			{
				case 0:
					UILinkPointNavigator.SetPosition(500, new Vector2((float)X - vector.X * (num2 / 2f * 0.8f), Y));
					break;
				case 1:
					UILinkPointNavigator.SetPosition(501, new Vector2((float)X - vector.X * (num2 / 2f * 0.8f), Y));
					break;
			}
			if (!flag)
			{
				UpdateHover(ID, hovering: false);
				return;
			}
			UpdateHover(ID, hovering: true);
			if (PlayerInput.IgnoreMouseInterface)
			{
				return;
			}
			player.mouseInterface = true;
			if (Main.mouseLeft && Main.mouseLeftRelease)
			{
				switch (ID)
				{
					case 0:
						DepositItems(true);
						break;
					case 1:
						DepositItems();
						break;
				}
				Recipe.FindRecipes();
			}
		}
	}
}
