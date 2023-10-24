﻿using System.Linq;
using Terraria;
using Terraria.UI;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.IO;
using Terraria.WorldBuilding;
using System;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using SatelliteStorage.DriveSystem;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.UI.Chat;
using Terraria.UI.Gamepad;
using Terraria.GameInput;
using Microsoft.Xna.Framework.Input;

namespace SatelliteStorage.UI
{
	public class DriveChestUI : BaseUIState
	{
		public static bool mouseOver;
		public static DriveChestUI instance;
		internal UserInterface DriveChestInterface;
		private GameTime _lastUpdateUiGameTime;
		private double lastCraftsResearchTime = 0;

		UIElements.UIItemsDisplay display;
		UIElements.UICraftDisplay craftDisplay;
		UIElements.UICraftRecipe craftRecipe;
		UIPanel craftResultPanel;

		private int defaultInventoryItemsCount;

		private float windowWidth;
		private float windowHeight;

		private Vector2 openPosition;
		private bool checkPosition = false;

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
		public static float[] ButtonScale = new float[2];
		public static bool[] ButtonHovered = new bool[2];

		private static Item itemOnMouse;

		public static bool isDrawing = false;

		public static int currentRecipe = -1;

		public static bool isMouseDownOnCraftItem = false;
		private double mouseDownOnCraftItemTime = 0;
		private bool mouseDownOnCraftItemToggle = false;
		private double mouseDownItemCraftCooldown = 0;

		public static void UpdateHover(int ID, bool hovering)
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

		public static void ReloadItems()
        {
			if (instance == null) return;
			if (!instance.GetState()) return;

			instance.display.RebuildPage();
		}


		public static void SetOpenedPosition(Vector2 pos)
        {
			instance.checkPosition = true;
			instance.openPosition = pos;
		}

		public static void ResetOpenedPosition()
		{
			instance.checkPosition = false;
		}

		/*
		public static void ReloadCraftItems()
        {
			List<DriveItem> driveItems = DriveChestSystem.GetItems();
			Item[] oldItems = new Item[instance.defaultInventoryItemsCount];
			for(int i = 0; i < instance.defaultInventoryItemsCount; i++)
            {
				oldItems[i] = Main.LocalPlayer.inventory[i];
            }
			Main.LocalPlayer.inventory = new Item[instance.defaultInventoryItemsCount + driveItems.Count];

			for(int i = 0; i < instance.defaultInventoryItemsCount; i++)
            {
				Main.LocalPlayer.inventory[i] = oldItems[i];
			}

			for (int i = 0; i < driveItems.Count; i++)
			{
				DriveItem driveItem = driveItems[i];
				Item item = new Item();
				item.type = driveItem.type;
				item.SetDefaults(item.type);
				item.stack = driveItem.stack;
				item.prefix = driveItem.prefix;

				Main.LocalPlayer.inventory[i+instance.defaultInventoryItemsCount] = item;
			}
		}
		*/

		public override void OnInitialize()
        {
            base.OnInitialize();

			CalculateSize();

			DriveChestInterface = new UserInterface();

			defaultInventoryItemsCount = Main.LocalPlayer.inventory.Length;

			display = new UIElements.UIItemsDisplay(this);
			
			Append(display);

			craftDisplay = new UIElements.UICraftDisplay(this);

			Append(craftDisplay);

			craftRecipe = new UIElements.UICraftRecipe(this);
			Append(craftRecipe);

			craftResultPanel = new UIElements.UICraftResultBG();
			Append(craftResultPanel);

			display.onFiltersChanged = () =>
			{
				craftDisplay._filterer = display._filterer;
				craftDisplay.UpdateContents();
			};

			CalculateSize();

			craftDisplay.onRecipeChoosen += (int recipe) =>
			{
				currentRecipe = recipe;
				craftRecipe.SetRecipe(recipe);
				SoundEngine.PlaySound(SoundID.MenuTick);
			};
			
			OnLeftMouseDown += (UIMouseEvent evt, UIElement listeningElement) =>
			{
				if (craftOnMouseRecipe > -1)
                {
					mouseDownOnCraftItemToggle = false;
					isMouseDownOnCraftItem = true;
					TryCraftItem();
				}
			};

			OnLeftMouseUp += (UIMouseEvent evt, UIElement listeningElement) =>
			{
				isMouseDownOnCraftItem = false;
			};
		}

		private void TryCraftItem()
        {
			if (craftOnMouseRecipe <= -1) return;

			if (DriveChestSystemLocal.CraftItem(craftOnMouseRecipe))
			{
                SoundEngine.PlaySound(SoundID.Grab);
                DriveChestSystem.checkRecipesRefresh = false;
            }
		}

		private void TryDepositItems(bool newItems = false)
        {
            if (DriveChestSystemLocal.DepositItemsFromInventory(newItems))
            {
                SoundEngine.PlaySound(SoundID.Grab);
                if (Main.netMode == NetmodeID.SinglePlayer) ReloadItems();
            }
        }

		/*
        private void OnPanelMouseDown(UIMouseEvent evt, UIElement listeningElement)
        {
			Player player = Main.LocalPlayer;
			Item mouseItem = player.inventory[58];

			if (mouseItem.IsAir || Main.mouseItem.IsAir) return;
            
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				if (!DriveChestSystem.AddItem(DriveItem.FromItem(Main.mouseItem))) return;
				ReloadItems();

				mouseItem.TurnToAir();
				Main.mouseItem.TurnToAir();
				SoundEngine.PlaySound(SoundID.Grab);
			}

			if (Main.netMode == NetmodeID.MultiplayerClient)
            {
				if (SatelliteStorage.AddDriveChestItemSended) return;
				SatelliteStorage.AddDriveChestItemSended = true;
				ModPacket packet = SatelliteStorage.instance.GetPacket();
				packet.Write((byte)SatelliteStorage.MessageType.AddDriveChestItem);
				packet.Write((byte)player.whoAmI);
				packet.Write((byte)0);

				packet.Send();
				packet.Close();
			}
			
		}
		*/


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

			if (craftDisplay != null && !UIElements.UICraftDisplay.hidden)
            {
				Terraria.Utils.DrawBorderString(spriteBatch, Language.GetTextValue("Mods.SatelliteStorage.UITitles.DriveChestRecipes"), craftDisplay.GetDimensions().Position() + new Vector2(30, 47), Color.White, 1f);
			}

			if (craftRecipe != null && !UIElements.UICraftRecipe.hidden)
			{
				Terraria.Utils.DrawBorderString(spriteBatch, Language.GetTextValue("Mods.SatelliteStorage.UITitles.DriveChestCraft"), craftRecipe.GetDimensions().Position() + new Vector2(30, 47), Color.White, 1f);
			}

			if (currentRecipe > -1 && !UIElements.UICraftRecipe.hidden)
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
				UIElements.UICraftResultBG.hidden = false;
			} else
            {
				UIElements.UICraftResultBG.hidden = true;
			}
			
		}


        public override void OnUpdateUI(GameTime gameTime)
		{

			isDrawing = false;

			if (DriveChestInterface?.CurrentState != null)
			{
				isDrawing = true;
				double elapsedTime = gameTime.TotalGameTime.TotalMilliseconds - lastCraftsResearchTime;
				
				_lastUpdateUiGameTime = gameTime;

				if (isMouseDownOnCraftItem)
                {
					if (!mouseDownOnCraftItemToggle)
					{
						mouseDownItemCraftCooldown = gameTime.TotalGameTime.TotalMilliseconds;
						mouseDownOnCraftItemTime = gameTime.TotalGameTime.TotalMilliseconds;
						mouseDownOnCraftItemToggle = true;
					}
				} else
                {
					mouseDownOnCraftItemToggle = false;
				}

				double mouseDownOnCraftItemElapsed = gameTime.TotalGameTime.TotalMilliseconds - mouseDownOnCraftItemTime;

				if (mouseDownOnCraftItemToggle && isMouseDownOnCraftItem)
                {
					double waitTime = 999999;
					if (mouseDownOnCraftItemElapsed >= 1000) waitTime = 200;
					if (mouseDownOnCraftItemElapsed >= 1500) waitTime = 150;
					if (mouseDownOnCraftItemElapsed >= 2000) waitTime = 100;
					if (mouseDownOnCraftItemElapsed >= 2500) waitTime = 50;
					if (mouseDownOnCraftItemElapsed >= 3000) waitTime = 16;

					if (gameTime.TotalGameTime.TotalMilliseconds > mouseDownItemCraftCooldown + waitTime)
                    {
						mouseDownItemCraftCooldown = gameTime.TotalGameTime.TotalMilliseconds;
						TryCraftItem();
					}

				}

				if (Main.recBigList) isDrawing = false;

				CalculateSize();
				DriveChestInterface.Update(gameTime);

				Player player = Main.LocalPlayer;
				Item mouseItem = player.inventory[58];

				if (mouseItem.IsAir || Main.mouseItem.IsAir)
				{
					if (itemOnMouse != null)
					{
						itemOnMouse = null;
						DriveChestSystem.checkRecipesRefresh = false;
					}
				}
				else
				{

					if (itemOnMouse == null || itemOnMouse.IsNotSameTypePrefixAndStack(mouseItem))
					{
						itemOnMouse = mouseItem;
						DriveChestSystem.checkRecipesRefresh = false;
					}
				}


				if (elapsedTime > 256 && (!DriveChestSystem.checkRecipesRefresh || SatelliteStoragePlayer.CheckAdjChanged()))
				{
					DriveChestSystem.checkRecipesRefresh = true;
					lastCraftsResearchTime = gameTime.TotalGameTime.TotalMilliseconds;
					DriveChestSystem.ResearchRecipes();
					instance.craftDisplay.RebuildPage();
					instance.craftRecipe.RebuildPage();
				}

				if (checkPosition)
				{
					if (Main.LocalPlayer.position.Distance(openPosition) > 100) SatelliteStorage.SetUIState((int)UITypes.DriveChest, false);
				}

				if (!Main.playerInventory) SatelliteStorage.SetUIState((int)UITypes.DriveChest, false);
				
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
						craftRecipe.Left.Set(craftDisplay.GetDimensions().X, 0);
						craftRecipe.Top.Set(craftDisplay.GetDimensions().Y - craftRecipeHeight + 25, 0);
						craftRecipe.Width.Set(craftDisplay.Width.Pixels, 0);
						craftRecipe.Height.Set(craftRecipeHeight, 0);

						craftRecipe.Recalculate();

						float craftResultWidth = craftRecipe.Width.Pixels * 0.25f;
						float craftResultHeight = craftRecipe.Width.Pixels * 0.25f;
						craftResultPanel.Left.Set(craftRecipe.GetDimensions().X + (craftRecipe.Width.Pixels / 2) - (craftResultWidth / 2), 0);
						craftResultPanel.Top.Set(craftRecipe.GetDimensions().Y - craftResultHeight + 30, 0);
						craftResultPanel.Width.Set(craftResultWidth, 0);
						craftResultPanel.Height.Set(craftResultHeight, 0);
					} else
                    {
						float craftRecipeWidth = MathF.Max(width * 0.1f, 150);
						float craftRecipeHeight = MathF.Max(craftDisplay.Height.Pixels * 0.55f, 150);
						craftRecipe.Left.Set(craftDisplay.GetDimensions().X - craftRecipeWidth - 15, 0);
						craftRecipe.Top.Set(craftDisplay.GetDimensions().Y, 0);
						craftRecipe.Width.Set(craftRecipeWidth, 0);
						craftRecipe.Height.Set(craftRecipeHeight, 0);

						craftRecipe.Recalculate();


						float craftResultWidth = craftRecipe.Width.Pixels * 0.35f;
						craftResultPanel.Left.Set(craftRecipe.GetDimensions().X + (craftRecipe.Width.Pixels / 2) - (craftResultWidth / 2), 0);
						craftResultPanel.Top.Set(craftRecipe.GetDimensions().Y + craftRecipe.Height.Pixels + 15, 0);
						craftResultPanel.Width.Set(craftRecipe.Width.Pixels * 0.35f, 0);
						craftResultPanel.Height.Set(craftRecipe.Width.Pixels * 0.35f, 0);
					}



					craftResultPanel.Recalculate();

					if (smallWidth)
                    {
						craftResultSlotPos = new Vector2(
							craftResultPanel.GetDimensions().X + (craftResultPanel.Width.Pixels / 2) - (40 / 2) - 1,
							craftResultPanel.GetDimensions().Y + (craftResultPanel.Height.Pixels / 2) - (40 / 2) + 1
						);
					} else
                    {
						craftResultSlotPos = new Vector2(
							craftResultPanel.GetDimensions().X + (craftResultPanel.Width.Pixels / 2) - (40 / 2),
							craftResultPanel.GetDimensions().Y + (craftResultPanel.Height.Pixels / 2) - (40 / 2)
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
						if (_lastUpdateUiGameTime != null && DriveChestInterface?.CurrentState != null)
						{
							DriveChestInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
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
				instance = this;
				DriveChestInterface?.SetState(this);
				CalculateSize();
				ReloadItems();
				DriveChestSystem.checkRecipesRefresh = false;
			}
			else
			{
				checkPosition = false;
				isMouseDownOnCraftItem = false;
				DriveChestInterface?.SetState(null);

				for (int i = 0; i < 2; i++)
				{
					ButtonScale[i] = 0.75f;
					ButtonHovered[i] = false;
				}

			}
		}

		public override bool GetState()
        {
			return DriveChestInterface?.CurrentState != null ? true : false;
		}

		private static void DrawButtons(SpriteBatch spritebatch)
		{
			if (instance.display == null) return;
			for (int i = 0; i < 2; i++)
			{
				DrawButton(spritebatch, i, (int)(instance.buttonsPos.X), (int)(instance.buttonsPos.Y));
			}
		}

		private static void DrawButton(SpriteBatch spriteBatch, int ID, int X, int Y)
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
						instance.TryDepositItems(true);
						break;
					case 1:
						instance.TryDepositItems();
						break;
				}
				Recipe.FindRecipes();
			}
		}
	}
}
