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
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using SatelliteStorage.DriveSystem;
using Terraria.GameContent.UI.Elements;
using Terraria;
using Microsoft.Xna.Framework.Input;
using Terraria.GameContent;
using Terraria.UI.Chat;

using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SatelliteStorage.UI
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
        private Dictionary<int, IDriveItem> _autoImportItems;
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
        private List<IDriveItem> _items;

        private UICreativeItemsInfiniteFilteringOptions uICreativeItemsInfiniteFilteringOptions;

        public event Action OnRequestItems;
        public event Action<IDriveItem, int> OnItemClicked;
        public event Action<IDriveItem> OnToggleItemAutoImport;

        public UIItemsDisplay(UIState uiStateThatHoldsThis)
        {
            _itemIdsAvailableTotal = new List<int>();
            _itemIdsAvailableToShow = new List<int>();
            _items = new List<IDriveItem>();
            _autoImportItems = new Dictionary<int, IDriveItem>();

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

            BuildPage();

            SatelliteStorageKeybinds.OnSearchItemName += SatelliteStorageKeybinds_OnSearchItemName;
        }

        public void RebuildPage()
        {
            UpdateContents();
        }

        private void RequestItems() => OnRequestItems?.Invoke();
        public void UpdateItems(List<IDriveItem> items) => _items = items;
        public void UpdateAutoImportItems(Dictionary<int, IDriveItem> autoImportItems)
            => _autoImportItems = autoImportItems;

        public int hoveredItemIndex =>
            _itemGrid != null ? _itemGrid.hoverItemIndex : -1;

        public Dictionary<int, IDriveItem> driveItems =>
            _itemGrid?._driveItems;

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
            OnUpdate += UICreativeInfiniteItemsDisplay_OnUpdate;
        }

        public void UpdateItemsTypes()
        {
            List<int> types = new List<int>();
            _items.ForEach(v => types.Add(v.type));
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

        private void CallOnItemClicked(IDriveItem item, int clickType) =>
            OnItemClicked?.Invoke(item, clickType);

        private void CallOnItemToggleAutoImport(IDriveItem item) =>
            OnToggleItemAutoImport(item);


        private void ItemClick(UIMouseEvent evt, UIElement listeningElement, int clickType)
        {
            if (_itemGrid.hoverItemIndex <= -1) return;
            IDriveItem driveItem = _itemGrid._driveItems[_itemGrid.hoverItemIndex];
            if (driveItem == null) return;
            CallOnItemClicked(driveItem, clickType);
        }

        private void BuildInfinitesMenuContents(UIElement totalContainer)
        {
            UIPanel uIPanel = CreateBasicPanel();
            totalContainer.Append(uIPanel);
            uIPanel.OnUpdate += Hover_OnUpdate;
            uIPanel.OnMouseOver += Hover_OnMouseOver;
            uIPanel.OnMouseOut += Hover_OnMouseOut;
            UIDynamicItemCollection item = _itemGrid = new UIDynamicItemCollection();

            item.SetDisplayAutoImport(true);

            item.OnLeftMouseDown += (evt, listeningElement) =>
            {
                if (isHoldingAltOnDriveChestItem)
                {
                    ToggleItemAutoImportClick();

                    return;
                }
                if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
                {
                    ItemClick(evt, listeningElement, 2);
                    return;
                }
                ItemClick(evt, listeningElement, 0);
                return;
            };

            item.OnRightMouseDown += (evt, listeningElement) =>
            {
                ItemClick(evt, listeningElement, 1);
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
            RequestItems();

            UpdateItemsTypes();
            _itemIdsAvailableToShow.Clear();

            _itemIdsAvailableToShow.AddRange(_itemIdsAvailableTotal.Where((x) =>
            {
                if (!ContentSamples.ItemsByType.ContainsKey(x)) return false;
                return _filterer.FitsFilter(ContentSamples.ItemsByType[x]);
            }));

            _itemIdsAvailableToShow.Sort(_sorter);
            _itemGrid.SetContentsToShow(_itemIdsAvailableToShow, _items, _autoImportItems);
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
            UIPanel uIPanel = _searchBoxPanel = new UIPanel
            {
                Width = new StyleDimension(0f - uIImageButton.Width.Pixels - 3f, 1f),
                Height = new StyleDimension(0f, 1f),
                VAlign = 0.5f,
                HAlign = 1f
            };
            uIPanel.BackgroundColor = new Color(35, 40, 83);
            uIPanel.BorderColor = new Color(35, 40, 83);
            uIPanel.SetPadding(0f);
            searchArea.Append(uIPanel);


            UISearchBar uISearchBar = _searchBar = new UISearchBar(Language.GetText("UI.PlayerNameSlot"), 0.8f)
            {
                Width = new StyleDimension(0f, 1f),
                Height = new StyleDimension(0f, 1f),
                HAlign = 0f,
                VAlign = 0.5f,
                Left = new StyleDimension(0f, 0f),
                IgnoresMouseInteraction = true
            };


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

        private void ToggleItemAutoImportClick()
        {
            if (_itemGrid.hoverItemIndex <= -1) return;

            IDriveItem driveItem = _itemGrid._driveItems[_itemGrid.hoverItemIndex];
            if (driveItem == null) return;

            CallOnItemToggleAutoImport(driveItem);
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