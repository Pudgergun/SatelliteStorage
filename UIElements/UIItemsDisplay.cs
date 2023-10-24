using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.GameContent.UI.States;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using SatelliteStorage.DriveSystem;
using Terraria.GameContent.UI.Elements;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;
using static System.Net.Mime.MediaTypeNames;
using Terraria.GameContent;
using Terraria.UI.Chat;
using System.Numerics;
using Terraria.UI.Gamepad;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SatelliteStorage.UIElements
{
	public class UIItemsDisplay : UIElement
	{
		public enum InfiniteItemsDisplayPage
		{
			InfiniteItemsPickup,
			InfiniteItemsResearch
		}

		public Action onFiltersChanged = null;
		private List<int> _itemIdsAvailableTotal;
		private List<int> _itemIdsAvailableToShow;
		private CreativeUnlocksTracker _lastTrackerCheckedForEdits;
		private int _lastCheckedVersionForEdits = -1;
		private UISearchBar _searchBar;
		private UIPanel _searchBoxPanel;
		private string _searchString;
		private UIDynamicItemCollection _itemGrid;
		public EntryFilterer<Item, IItemEntryFilter> _filterer;
		private EntrySorter<int, ICreativeItemSortStep> _sorter;
		private UIElement _containerInfinites;
		private UIElement _containerSacrifice;
		private bool _showSacrificesInsteadOfInfinites;
		public const string SnapPointName_SacrificeSlot = "CreativeSacrificeSlot";
		public const string SnapPointName_SacrificeConfirmButton = "CreativeSacrificeConfirm";
		public const string SnapPointName_InfinitesFilter = "CreativeInfinitesFilter";
		public const string SnapPointName_InfinitesSearch = "CreativeInfinitesSearch";
		public const string SnapPointName_InfinitesItemSlot = "CreativeInfinitesSlot";
		private int _sacrificeAnimationTimeLeft;
		private bool _hovered;
		private bool _didClickSomething;
		private bool _didClickSearchBar;
		private UICreativeItemsInfiniteFilteringOptions uICreativeItemsInfiniteFilteringOptions;

		public UIItemsDisplay(UIState uiStateThatHoldsThis)
		{
			_itemIdsAvailableTotal = new List<int>();
			_itemIdsAvailableToShow = new List<int>();
			_filterer = new EntryFilterer<Item, IItemEntryFilter>();
			List<IItemEntryFilter> list = new List<IItemEntryFilter>
			{
				new ItemFilters.Weapon(),
				new ItemFilters.Armor(),
				new ItemFilters.Vanity(),
				new ItemFilters.BuildingBlock(),
				new ItemFilters.Furniture(),
				new ItemFilters.Accessories(),
				new ItemFilters.MiscAccessories(),
				new ItemFilters.Consumables(),
				new ItemFilters.Tools(),
				new ItemFilters.Materials()
			};
			List<IItemEntryFilter> list2 = new List<IItemEntryFilter>();
			list2.AddRange(list);
			_filterer.AddFilters(list2);
			_filterer.SetSearchFilterObject(new ItemFilters.BySearch());
			_sorter = new EntrySorter<int, ICreativeItemSortStep>();
			_sorter.AddSortSteps(new List<ICreativeItemSortStep>
			{
				new SortingSteps.ByCreativeSortingId(),
				new SortingSteps.Alphabetical()
			});


			OnLeftMouseDown += (UIMouseEvent evt, UIElement listeningElement) =>
			{
				Player player = Main.LocalPlayer;
				Item mouseItem = player.inventory[58];

				if (mouseItem.IsAir || Main.mouseItem.IsAir) return;

				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					if (!DriveChestSystem.AddItem(DriveItem.FromItem(Main.mouseItem))) return;
					UI.DriveChestUI.ReloadItems();

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
					packet.Send();
					packet.Close();
				}
			};

			BuildPage();

            SatelliteStorageKeybinds.OnSearchItemName += SatelliteStorageKeybinds_OnSearchItemName;
        }

		public void RebuildPage()
        {
			UpdateContents();
		}


		private void BuildPage()
		{
			_lastCheckedVersionForEdits = -1;
			RemoveAllChildren();
			SetPadding(0f);
			UIElement uIElement = new UIElement
			{
				Width = StyleDimension.Fill,
				Height = StyleDimension.Fill
			};
			uIElement.SetPadding(0f);
			_containerInfinites = uIElement;
			UIElement uIElement2 = new UIElement
			{
				Width = StyleDimension.Fill,
				Height = StyleDimension.Fill
			};
			uIElement2.SetPadding(0f);
			_containerSacrifice = uIElement2;

			BuildInfinitesMenuContents(uIElement);			
			UpdateContents();
			base.OnUpdate += UICreativeInfiniteItemsDisplay_OnUpdate;
		}

		public void UpdateItemsTypes()
		{
			List<int> types = new List<int>();
			DriveChestSystem.GetItems().ForEach(v => types.Add(v.type));
			_itemIdsAvailableTotal.Clear();
			_itemIdsAvailableTotal.AddRange(types);
		}

		private void Hover_OnUpdate(UIElement affectedElement)
		{
			if (_hovered)
			{
				Main.LocalPlayer.mouseInterface = true;
			}
		}

		private void Hover_OnMouseOut(UIMouseEvent evt, UIElement listeningElement)
		{
			_hovered = false;
		}

		private void Hover_OnMouseOver(UIMouseEvent evt, UIElement listeningElement)
		{
			_hovered = true;
		}

		private static UIPanel CreateBasicPanel()
		{
			UIPanel uIPanel = new UIPanel();
			SetBasicSizesForCreativeSacrificeOrInfinitesPanel(uIPanel);
			uIPanel.BackgroundColor *= 0.8f;
			uIPanel.BorderColor *= 0.8f;
			return uIPanel;
		}

		private static void SetBasicSizesForCreativeSacrificeOrInfinitesPanel(UIElement element)
		{
			element.Width = new StyleDimension(0f, 1f);
			element.Height = new StyleDimension(-38f, 1f);
			element.Top = new StyleDimension(38f, 0f);
		}

		private void TakeItem(UIMouseEvent evt, UIElement listeningElement, int clickType)
		{
			if (_itemGrid.hoverItemIndex <= -1) return;
			DriveItem driveItem = _itemGrid._driveItems[_itemGrid.hoverItemIndex];
			if (driveItem == null) return;
			Item hoverItem = driveItem.ToItem();

			Player player = Main.LocalPlayer;
			Item mouseItem = player.inventory[58];
			int slotToAdd = -1;
			int countToAdd = 0;

			bool isMouseItemAir = mouseItem.IsAir && Main.mouseItem.IsAir;
			bool isMouseItemSame = mouseItem.type == driveItem.type;
			if (!isMouseItemAir && !isMouseItemSame) return;

			if (clickType == 1)
			{
				if (!isMouseItemAir && !isMouseItemSame) return;

				if (isMouseItemSame)
				{
					if (mouseItem.stack + 1 > mouseItem.maxStack) return;
				}
			}
			else if (clickType == 2)
			{

				if (!isMouseItemAir) return;

				int stackDiff = 0;

				for (int s = 0; s < 50; s++)
				{
					if (player.inventory[s].type != ItemID.None &&
						player.inventory[s].type == hoverItem.type &&
						player.inventory[s].stack < hoverItem.maxStack &&
						hoverItem.maxStack - player.inventory[s].stack > stackDiff &&
						!player.inventory[s].favorited
					)
					{
						stackDiff = hoverItem.maxStack - player.inventory[s].stack;
						countToAdd = stackDiff;
						if (countToAdd > hoverItem.stack) countToAdd = hoverItem.stack;
						slotToAdd = s;
					}
				}

				if (slotToAdd <= -1)
				{
					for (int s = 0; s < 50; s++)
					{
						if (player.inventory[s].type == ItemID.None)
						{
							slotToAdd = s;
							countToAdd = hoverItem.maxStack;
							if (countToAdd > hoverItem.stack) countToAdd = hoverItem.stack;
							break;
						}
					}
				}
			} else
			{
				if (!isMouseItemAir) return;
			}

			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				if (clickType == 2 && slotToAdd <= -1) return;

				int takeCount = clickType == 1 ? 1 : 0;
				if (clickType == 2) takeCount = countToAdd;

				Item takeItem = DriveChestSystem.TakeItem(driveItem.type, driveItem.prefix, takeCount);
				if (takeItem == null) return;

				if (clickType == 1)
				{
					if (isMouseItemAir)
					{
						Main.mouseItem = takeItem.Clone();
					}
					else
					{
						Main.mouseItem.stack += 1;
					}
				}
				else if (clickType == 2)
				{
					if (player.inventory[slotToAdd].type == ItemID.None)
                    {
						player.inventory[slotToAdd] = takeItem.Clone();
					} else
                    {
						player.inventory[slotToAdd].stack += countToAdd;
					}

					SoundEngine.PlaySound(SoundID.Grab);
				} else
				{
					Main.mouseItem = takeItem;
				}

				UI.DriveChestUI.ReloadItems();

				if (clickType == 1)
				{
					SoundEngine.PlaySound(SoundID.MenuTick);
				}
				else
				{
					SoundEngine.PlaySound(SoundID.Grab);
				}
			}

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				if (SatelliteStorage.TakeDriveChestItemSended) return;
				SatelliteStorage.TakeDriveChestItemSended = true;

				ModPacket packet = SatelliteStorage.instance.GetPacket();
				packet.Write((byte)SatelliteStorage.MessageType.TakeDriveChestItem);
				packet.Write((byte)player.whoAmI);
				packet.Write((byte)clickType);
				packet.Write7BitEncodedInt(driveItem.type);
				packet.Write7BitEncodedInt(driveItem.prefix);
				packet.Write7BitEncodedInt(0);
				packet.Send();
				packet.Close();
			}

			return;
		}

		private void BuildInfinitesMenuContents(UIElement totalContainer)
		{
			UIPanel uIPanel = CreateBasicPanel();
			totalContainer.Append(uIPanel);
			uIPanel.OnUpdate += Hover_OnUpdate;
			uIPanel.OnMouseOver += Hover_OnMouseOver;
			uIPanel.OnMouseOut += Hover_OnMouseOut;
			UIDynamicItemCollection item = (_itemGrid = new UIDynamicItemCollection());

			item.SetDisplayAutoImport(true);


            item.OnLeftMouseDown += (UIMouseEvent evt, UIElement listeningElement) =>
			{
				if (isHoldingAltOnDriveChestItem)
				{
					ToggleHoveredItemAutoImport();

                    return;
				}
				if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
                {
					TakeItem(evt, listeningElement, 2);
					return;
                }
				TakeItem(evt, listeningElement, 0);
				return;
			};

			item.OnRightMouseDown += (UIMouseEvent evt, UIElement listeningElement) =>
			{
				TakeItem(evt, listeningElement, 1);
				return;
			};

			UIElement uIElement = new UIElement
			{
				Height = new StyleDimension(24f, 0f),
				Width = new StyleDimension(0f, 1f)
			};
			uIElement.SetPadding(0f);
			uIPanel.Append(uIElement);
			AddSearchBar(uIElement);
			_searchBar.SetContents(null, forced: true);
			UIList uIList = new UIList
			{
				Width = new StyleDimension(-25f, 1f),
				Height = new StyleDimension(-28f, 1f),
				VAlign = 1f,
				HAlign = 0f
			};
			uIPanel.Append(uIList);
			float num = 4f;
			UIScrollbar uIScrollbar = new UIScrollbar
			{
				Height = new StyleDimension(-28f - num * 2f, 1f),
				Top = new StyleDimension(0f - num, 0f),
				VAlign = 1f,
				HAlign = 1f
			};
			uIPanel.Append(uIScrollbar);
			uIList.SetScrollbar(uIScrollbar);
			uIList.Add(item);
			uICreativeItemsInfiniteFilteringOptions = new UICreativeItemsInfiniteFilteringOptions(_filterer, "CreativeInfinitesFilter");
			uICreativeItemsInfiniteFilteringOptions.OnClickingOption += filtersHelper_OnClickingOption;
			uICreativeItemsInfiniteFilteringOptions.Left = new StyleDimension(20f, 0f);
			totalContainer.Append(uICreativeItemsInfiniteFilteringOptions);
			uICreativeItemsInfiniteFilteringOptions.OnUpdate += Hover_OnUpdate;
			uICreativeItemsInfiniteFilteringOptions.OnMouseOver += Hover_OnMouseOver;
			uICreativeItemsInfiniteFilteringOptions.OnMouseOut += Hover_OnMouseOut;
		}

		private void UpdateSacrificeAnimation()
		{
			if (_sacrificeAnimationTimeLeft > 0)
			{
				_sacrificeAnimationTimeLeft--;
			}
		}

		public void SetPageTypeToShow(InfiniteItemsDisplayPage page)
		{
			_showSacrificesInsteadOfInfinites = page == InfiniteItemsDisplayPage.InfiniteItemsResearch;
		}

		private void UICreativeInfiniteItemsDisplay_OnUpdate(UIElement affectedElement)
		{
			RemoveAllChildren();
			CreativeUnlocksTracker localPlayerCreativeTracker = Main.LocalPlayerCreativeTracker;
			if (_lastTrackerCheckedForEdits != localPlayerCreativeTracker)
			{
				_lastTrackerCheckedForEdits = localPlayerCreativeTracker;
				_lastCheckedVersionForEdits = -1;
			}
			int lastEditId = localPlayerCreativeTracker.ItemSacrifices.LastEditId;
			if (_lastCheckedVersionForEdits != lastEditId)
			{
				_lastCheckedVersionForEdits = lastEditId;
				UpdateContents();
			}
			if (_showSacrificesInsteadOfInfinites)
			{
				Append(_containerSacrifice);
			}
			else
			{
				Append(_containerInfinites);
			}
			UpdateSacrificeAnimation();
		}

		private void filtersHelper_OnClickingOption()
		{
			if (onFiltersChanged != null) onFiltersChanged.Invoke();
			UpdateContents();
		}

		private void UpdateContents()
		{
			UpdateItemsTypes();
			_itemIdsAvailableToShow.Clear();

			_itemIdsAvailableToShow.AddRange(_itemIdsAvailableTotal.Where((int x) =>
			{
				if (!ContentSamples.ItemsByType.ContainsKey(x)) return false;
				return _filterer.FitsFilter(ContentSamples.ItemsByType[x]);
			}));

			_itemIdsAvailableToShow.Sort(_sorter);
			_itemGrid.SetContentsToShow(_itemIdsAvailableToShow, DriveChestSystem.GetItems());
		}

		private void AddSearchBar(UIElement searchArea)
		{
			UIImageButton uIImageButton = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Button_Search", (AssetRequestMode)1))
			{
				VAlign = 0.5f,
				HAlign = 0f
			};
			uIImageButton.OnLeftClick += Click_SearchArea;
			uIImageButton.SetHoverImage(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Button_Search_Border", (AssetRequestMode)1));
			uIImageButton.SetVisibility(1f, 1f);
			uIImageButton.SetSnapPoint("CreativeInfinitesSearch", 0);
			searchArea.Append(uIImageButton);
			UIPanel uIPanel = (_searchBoxPanel = new UIPanel
			{
				Width = new StyleDimension(0f - uIImageButton.Width.Pixels - 3f, 1f),
				Height = new StyleDimension(0f, 1f),
				VAlign = 0.5f,
				HAlign = 1f
			});
			uIPanel.BackgroundColor = new Color(35, 40, 83);
			uIPanel.BorderColor = new Color(35, 40, 83);
			uIPanel.SetPadding(0f);
			searchArea.Append(uIPanel);


			UISearchBar uISearchBar = (_searchBar = new UISearchBar(Language.GetText("UI.PlayerNameSlot"), 0.8f)
			{
				Width = new StyleDimension(0f, 1f),
				Height = new StyleDimension(0f, 1f),
				HAlign = 0f,
				VAlign = 0.5f,
				Left = new StyleDimension(0f, 0f),
				IgnoresMouseInteraction = true
			});


            uIPanel.OnLeftClick += Click_SearchArea;
			uISearchBar.OnContentsChanged += OnSearchContentsChanged;
			uIPanel.Append(uISearchBar);
			uISearchBar.OnStartTakingInput += OnStartTakingInput;
			uISearchBar.OnEndTakingInput += OnEndTakingInput;
			uISearchBar.OnNeedingVirtualKeyboard += OpenVirtualKeyboardWhenNeeded;
			uISearchBar.OnCanceledTakingInput += OnCancledInput;
			UIImageButton uIImageButton2 = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/SearchCancel", (AssetRequestMode)1))
			{
				HAlign = 1f,
				VAlign = 0.5f,
				Left = new StyleDimension(-2f, 0f)
			};
			uIImageButton2.OnMouseOver += searchCancelButton_OnMouseOver;
			uIImageButton2.OnLeftClick += searchCancelButton_OnClick;
			uIPanel.Append(uIImageButton2);
		}

        private void SatelliteStorageKeybinds_OnSearchItemName(object sender, string name)
        {
			_searchBar?.SetContents(name);
        }

        private void searchCancelButton_OnClick(UIMouseEvent evt, UIElement listeningElement)
		{
			if (_searchBar.HasContents)
			{
				_searchBar.SetContents(null, forced: true);
				SoundEngine.PlaySound(SoundID.MenuTick);
			}
			else
			{
				SoundEngine.PlaySound(SoundID.MenuTick);
			}
		}

		private void searchCancelButton_OnMouseOver(UIMouseEvent evt, UIElement listeningElement)
		{
			SoundEngine.PlaySound(SoundID.MenuTick);
		}

		private void OnCancledInput()
		{
			Main.LocalPlayer.ToggleInv();
		}

		private void Click_SearchArea(UIMouseEvent evt, UIElement listeningElement)
		{
			if (evt.Target.Parent != _searchBoxPanel)
			{
				_searchBar.ToggleTakingText();
				_didClickSearchBar = true;
			}
		}

		public override void LeftClick(UIMouseEvent evt)
		{
			AttemptStoppingUsingSearchbar(evt);
		}

		private void AttemptStoppingUsingSearchbar(UIMouseEvent evt)
		{
			_didClickSomething = true;
		}

		private bool isHoldingAltOnDriveChestItem => _itemGrid.hoverItemIndex > -1 && !Main.HoverItem.IsAir && Main.keyState.IsKeyDown(Keys.LeftAlt);

		private void ToggleHoveredItemAutoImport()
		{
            if (_itemGrid.hoverItemIndex <= -1) return;

            DriveItem driveItem = _itemGrid._driveItems[_itemGrid.hoverItemIndex];
            if (driveItem == null) return;

            if (SatelliteStorage.AutoImportItems.ContainsKey(driveItem.type))
                SatelliteStorage.AutoImportItems.Remove(driveItem.type);
            else
                SatelliteStorage.AutoImportItems.Add(driveItem.type, driveItem);

            SoundEngine.PlaySound(SoundID.MenuTick);

            UI.DriveChestUI.ReloadItems();
        }

        public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if (_didClickSomething && !_didClickSearchBar && _searchBar.IsWritingText)
			{
				_searchBar.ToggleTakingText();
			}
			_didClickSomething = false;
			_didClickSearchBar = false;

            if (isHoldingAltOnDriveChestItem)
            {
                Main.cursorOverride = CursorOverrideID.FavoriteStar;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (isHoldingAltOnDriveChestItem)
            {
				DrawItemsAutoImportText(spriteBatch);
            }
        }

		private static void DrawItemsAutoImportText(SpriteBatch spriteBatch)
		{
            Color color = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);
			string text = Language.GetTextValue("Mods.SatelliteStorage.UITitles.AutoImport");
            Vector2 vector = FontAssets.MouseText.Value.MeasureString(text);

            int X = Main.mouseX + (int)vector.X - 25, Y = Main.mouseY - (int)vector.Y + 25;

            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, new Vector2(X, Y), color, 0f, vector / 2f, new Vector2(1), -1f, 1.5f);
        }


        private void OnSearchContentsChanged(string contents)
		{
			_searchString = contents;
			_filterer.SetSearchFilter(contents);
			UpdateContents();
			if (onFiltersChanged != null) onFiltersChanged.Invoke();
		}

		private void OnStartTakingInput()
		{
			_searchBoxPanel.BorderColor = Main.OurFavoriteColor;
		}

		private void OnEndTakingInput()
		{
			_searchBoxPanel.BorderColor = new Color(35, 40, 83);
		}

		private void OpenVirtualKeyboardWhenNeeded()
		{
			int maxInputLength = 40;
			UIVirtualKeyboard uIVirtualKeyboard = new UIVirtualKeyboard(Language.GetText("UI.PlayerNameSlot").Value, _searchString, OnFinishedSettingName, GoBackHere, 3, allowEmpty: true);
			uIVirtualKeyboard.SetMaxInputLength(maxInputLength);
			IngameFancyUI.OpenUIState(uIVirtualKeyboard);
		}

		private static UserInterface GetCurrentInterface()
		{
			UserInterface activeInstance = UserInterface.ActiveInstance;
			if (Main.gameMenu)
			{
				return Main.MenuUI;
			}
			return Main.InGameUI;
		}

		private void OnFinishedSettingName(string name)
		{
			string contents = name.Trim();
			_searchBar.SetContents(contents);
			GoBackHere();
		}

		private void GoBackHere()
		{
			IngameFancyUI.Close();
			_searchBar.ToggleTakingText();
			Main.CreativeMenu.GamepadMoveToSearchButtonHack = true;
		}

		public int GetItemsPerLine()
		{
			return _itemGrid.GetItemsPerLine();
		}

        public override void Recalculate()
        {
            base.Recalculate();
			if (_itemGrid != null) _itemGrid.Recalculate();
        }
    }
}