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
            SearchItemKeybind = KeybindLoader.RegisterKeybind(Mod, "SearchItem", "O");
            QuickOpenDrive = KeybindLoader.RegisterKeybind(Mod, "QuickOpen", "E");
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
            {
                if (!Main.HoverItem.IsAir)
                {
                    SatelliteStorageKeybinds.InvokeSearchItemKeybind(Main.HoverItem.AffixName());
                }
            }

            if (
                SatelliteStorageKeybindsSystem.QuickOpenDrive.JustPressed &&
                Main.LocalPlayer.GetModPlayer<SatelliteStoragePlayer>().hasDriveRemoteItem
            )
            {
                if (Main.gameTimeCache.TotalGameTime.TotalMilliseconds - quickOpenCooldownMilliseconds >= 500)
                {
                    quickOpenCooldownMilliseconds = Main.gameTimeCache.TotalGameTime.TotalMilliseconds;

                    if (!DriveSystem.DriveChestSystem.isSputnikPlaced)
                    {
                        Main.NewText(Language.GetTextValue("Mods.SatelliteStorage.Common.CantUseWithoutSputnik"), new Color(173, 57, 71));
                    }

                    if (!SatelliteStorage.GetUIState((int)UI.UITypes.DriveChest)) DriveSystem.DriveChestSystem.RequestOpenDriveChest();
                }
            }
        }
    }
}
