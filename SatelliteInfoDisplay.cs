using Microsoft.Xna.Framework;
using SatelliteStorage.DriveSystem;
using SatelliteStorage.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SatelliteStorage
{
    internal class SatelliteInfoDisplay : InfoDisplay
    {

        public override bool Active()
		{
			return Main.LocalPlayer.GetModPlayer<SatelliteStoragePlayer>().showItemsCount;
		}

        public override string DisplayValue(ref Color displayColor, ref Color displayShadowColor)
		{
			string count = SatelliteStorage.driveChestSystem.itemsCount.ToString("##,##0");

			if (Language.GetTextValue("Mods.SatelliteStorage.Common.Lang") == "ru")
				count = SatelliteStorage.driveChestSystem.itemsCount +"";

			return $"{count} " + Language.GetTextValue("Mods.SatelliteStorage.Common.ItemsCounterCount");
		}
	}
}
