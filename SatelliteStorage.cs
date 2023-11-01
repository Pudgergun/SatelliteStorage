using Terraria.ModLoader;
using Terraria;
using Terraria.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;
using SatelliteStorage.DriveSystem;
using System;
using Terraria.Audio;
using SatelliteStorage.DriveChestNetwork;
using SatelliteStorage.Generators;
using SatelliteStorage.ModNetwork;
using SatelliteStorage.GeneratorsNetwork;
using SatelliteStorage.UI;
using System.Linq;

namespace SatelliteStorage
{

    public partial class SatelliteStorage : Mod
	{
        private static SatelliteStorage instance;

        private DriveChestSystem _driveChestSystem;
        private GeneratorsSystem _generatorsSystem;
        private UIDriveChest _driveChestUI;
        public static DriveChestSystem driveChestSystem => instance._driveChestSystem;
        public static GeneratorsSystem generatorsSystem => instance._generatorsSystem;
        public static UIDriveChest driveChestUI => instance._driveChestUI;

        public const int GeneratorsInterval = 20000;
        public const int ModVersion = 1;
        public const bool DebugMode = true;

        private ISatelliteStorageNet _driveChestNet;
        private ISatelliteStorageNet _generatorsNet;

        private IDriveChestPositionState _driveChestPositionState;

        private Dictionary<int, UIBaseState> uidict = new Dictionary<int, UIBaseState>();
        

        private double recipesResearchTime = 0;

        public override void Load()
        {
            instance = this;

            if (!Main.dedServ)
            {
                uidict.Add((int)UITypes.DriveChest, _driveChestUI = new UIDriveChest());
            }

            _driveChestSystem = new DriveChestSystem(this, _driveChestUI);
            _generatorsSystem = new GeneratorsSystem(this);

            _generatorsSystem.OnItemGenerated += (type, count) =>
            {
                _driveChestSystem.AddItem(
                    new DriveItem()
                        .SetType(type)
                        .SetStack(count)
                );
            };

            _driveChestPositionState = new DriveChestPositionState();

            base.Load();

            if (!Main.dedServ)
            {

                foreach(UIBaseState ui in uidict.Values)
                {
                    ui.Activate();
                }

                _driveChestSystem.OnDriveChestOpened += (positionChecking, position) =>
                {
                    if (positionChecking)
                        _driveChestPositionState.SetOpenedPosition(position);
                };

                _driveChestUI.OnDisplayItemClicked += (item, clickType) =>
                    _driveChestSystem.TakeItemClicked(item, clickType);

                _driveChestUI.OnDisplayItemToggleAutoImport += (item) =>
                {
                    Main.LocalPlayer.GetModPlayer<SatelliteStoragePlayer>()
                        .ToggleItemAutoImportClicked(item);

                    SoundEngine.PlaySound(SoundID.MenuTick);
                    _driveChestUI.ReloadItems();
                };

                _driveChestUI.OnItemsDisplayLeftMouseDown += () =>
                    _driveChestSystem.AddItemFromMouse();

                _driveChestUI.OnRequestItems += () =>
                {
                    _driveChestUI.UpdateAvailableRecipes(_driveChestSystem.GetAvailableRecipes());
                    _driveChestUI.UpdateItems(_driveChestSystem.GetItems());

                    _driveChestUI.UpdateAutoImportItems(
                        Main.LocalPlayer.GetModPlayer<SatelliteStoragePlayer>().GetAutoImportItems()
                    );
                };

                _driveChestUI.OnCheckRecipesRefreshState += (state) =>
                    _driveChestSystem.checkRecipesRefresh = state;

                _driveChestUI.OnCraftItem += (recipe) =>
                {
                    if (!_driveChestSystem.CraftItem(recipe)) return;

                    SoundEngine.PlaySound(SoundID.Grab);
                    _driveChestSystem.checkRecipesRefresh = false;
                };

                _driveChestUI.OnDepositItems += (allowNewItems) =>
                {
                    if (!_driveChestSystem.DepositItemsFromInventory(allowNewItems)) return;
                    
                    SoundEngine.PlaySound(SoundID.Grab);

                    if (Main.netMode == NetmodeID.SinglePlayer) 
                        _driveChestUI.ReloadItems();
                };
            }
        }

        public override void Unload()
        {
            base.Unload();
        }

        public static void SetUIState(int type, bool state)
        {
            if (instance.uidict.ContainsKey(type))
                instance.uidict[type].SetState(state);

        }

        public static bool GetUIState(int type)
        {
            return instance.uidict.TryGetValue(type, out var e) && e.GetState();
        }

        public static void Debug(string msg)
        {
            instance.Logger.Debug(msg);
        }

        public static void UpdateUI(GameTime gameTime) 
            => instance.OnUpdateUI(gameTime);

        public void OnUpdateUI(GameTime gameTime)
        {

            foreach (UIBaseState ui in uidict.Values)
            {
                ui.OnUpdateUI(gameTime);
            }

            if (
                gameTime.TotalGameTime.TotalMilliseconds - recipesResearchTime > 256 &&
                (!_driveChestSystem.checkRecipesRefresh || SatelliteStoragePlayer.CheckAdjChanged()))
            {
                _driveChestSystem.checkRecipesRefresh = true;
                recipesResearchTime = gameTime.TotalGameTime.TotalMilliseconds;
                _driveChestSystem.ResearchRecipes();
                _driveChestUI.RebuildCraftingPages();
            }

            if (_driveChestUI.GetState())
            {
                if (_driveChestPositionState.positionChecking)
                {
                    if (Main.LocalPlayer.position.Distance(_driveChestPositionState.openedPosition) > 100) SetUIState((int)UITypes.DriveChest, false);
                }

                if (!Main.playerInventory) SetUIState((int)UITypes.DriveChest, false);
            } else
            {
                if (_driveChestPositionState.positionChecking)
                    _driveChestPositionState.ResetOpenedPosition();
            }
        }

        public static void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
            => instance.OnModifyInterfaceLayers(layers);

        public void OnModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            foreach (UI.UIBaseState ui in uidict.Values)
            {
                ui.OnModifyInterfaceLayers(layers);
            }
        }
    

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            if (_driveChestNet == null)
            {
                if (Main.netMode == NetmodeID.Server)
                {
                    _driveChestNet = new DriveChestServer(this, _driveChestSystem);
                    _generatorsNet = new GeneratorsServer(this, _generatorsSystem);
                }

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    _driveChestNet = new DriveChestClient(this, _driveChestSystem, _driveChestUI);
                    _generatorsNet = new GeneratorsClient(this, _generatorsSystem);
                }
            }

            IPacketContext eventContext = new PacketContext(reader, whoAmI) { logsEnabled = false };

            _driveChestNet.HandlePacket(eventContext);
            _generatorsNet.HandlePacket(eventContext);
        }
    }
}