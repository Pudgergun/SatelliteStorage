using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SatelliteStorage
{
    internal class SatelliteInfoDisplay : InfoDisplay
    {
		private string ItemsCounterText;

		public override void SetStaticDefaults()
		{
			InfoName.SetDefault(Language.GetTextValue("Mods.SatelliteStorage.Common.ItemsCounter"));
		}

		public override bool Active()
		{
			return Terraria.Main.LocalPlayer.GetModPlayer<SatelliteDisplayPlayer>().showItemsCount;
		}

		public override string DisplayValue()
		{
			string count = SatelliteStorage.itemsCount.ToString("##,##0");

			if (Language.GetTextValue("Mods.SatelliteStorage.Common.Lang") == "ru")
				count = SatelliteStorage.itemsCount +"";

			return $"{count} " + Language.GetTextValue("Mods.SatelliteStorage.Common.ItemsCounterCount");
		}
	}

	public class SatelliteDisplayPlayer : ModPlayer
	{
		public bool showItemsCount;

		public override void ResetEffects()
		{
			showItemsCount = false;
		}

		public override void UpdateEquips()
		{
			foreach(Terraria.Item item in Player.armor)
            {
				if (item.type == ModContent.ItemType<Items.ItemsCountAccessoryItem>())
                {
					showItemsCount = true;
				}

			}

			if (Player.HasItem(ModContent.ItemType<Items.ItemsCountAccessoryItem>()))
				showItemsCount = true;
		}
	}
}
