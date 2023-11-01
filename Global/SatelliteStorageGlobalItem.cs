using SatelliteStorage.DriveSystem;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace SatelliteStorage.Global
{
    class SatelliteStorageGlobalItem : GlobalItem
    {
        public override bool OnPickup(Item item, Player player)
        {
            if (SatelliteStorage.GetUIState((int)UI.UITypes.DriveChest))
            {
                SatelliteStorage.driveChestSystem.checkRecipesRefresh = false;
            }

            return base.OnPickup(item, player);
        }

        public override void OnConsumeItem(Item item, Player player)
        {
            if (SatelliteStorage.GetUIState((int)UI.UITypes.DriveChest))
            {
                SatelliteStorage.driveChestSystem.checkRecipesRefresh = false;
            }

            base.OnConsumeItem(item, player);
        }
    }
}
