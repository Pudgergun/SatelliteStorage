using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameInput;
using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using SatelliteStorage.Utils;
using SatelliteStorage.DriveSystem;

namespace SatelliteStorage
{
    class SatelliteStorageKeybinds
    {
        public static event EventHandler<string> OnSearchItemName;

        public static void InvokeSearchItemKeybind(string name)
        {
            OnSearchItemName.Invoke(null, name);
        }
    }

    class SatelliteStorageKeybindsSystem : ModSystem
    {
        public static ModKeybind SearchItemKeybind { get; private set; }
        public static ModKeybind QuickOpenDrive { get; private set; }

        public override void Load()
        {
            SearchItemKeybind = KeybindLoader.RegisterKeybind(Mod, "SearchItem", "P");
            QuickOpenDrive = KeybindLoader.RegisterKeybind(Mod, "QuickOpen", "O");
        }

        public override void Unload()
        {
            SearchItemKeybind = null;
            QuickOpenDrive = null;
        }
    }

    class SatelliteStorageKeybindsPlayer : ModPlayer
    {
        private static double quickOpenCooldownMilliseconds;


        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (SatelliteStorageKeybindsSystem.SearchItemKeybind.JustPressed)
                OnSearchItemPressed();

            if (SatelliteStorageKeybindsSystem.QuickOpenDrive.JustPressed)
                OnQuickOpenPressed();
        }

        private void OnQuickOpenPressed()
        {
            if (!Main.LocalPlayer.GetModPlayer<SatelliteStoragePlayer>().hasDriveRemoteItem) return;
            if (Main.gameTimeCache.TotalGameTime.TotalMilliseconds - quickOpenCooldownMilliseconds < 500) return;

            quickOpenCooldownMilliseconds = Main.gameTimeCache.TotalGameTime.TotalMilliseconds;

            if (!SatelliteStorage.driveChestSystem.isSputnikPlaced)
            {
                Main.NewText(
                    Language.GetTextValue("Mods.SatelliteStorage.Common.CantUseWithoutSputnik"),
                    new Color(173, 57, 71)
                );

                return;
            }

            if (!SatelliteStorage.GetUIState((int)UI.UITypes.DriveChest))
                SatelliteStorage.driveChestSystem.ToggleDriveChestMenu();
        }

        private void OnSearchItemPressed()
        {
            if (Main.HoverItem.IsAir) return;

            SatelliteStorageKeybinds.InvokeSearchItemKeybind(
                StringUtils.CleanAffixName(Main.HoverItem.AffixName())
            );
        }
    }
}
