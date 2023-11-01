using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace SatelliteStorage.Tiles
{
    class QuartzCrateTile : ModTile
    {
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileTable[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.CoordinateHeights = new int[2] { 16, 18 };
			TileObjectData.newTile.CoordinateWidth = 17;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.addTile(Type);

			LocalizedText name = CreateMapEntryName();
			name.Format(Language.GetTextValue("Mods.SatelliteStorage.Tiles.QuartzCrateTile"));
			AddMapEntry(new Color(133, 50, 168), name);
		}

		public override bool CreateDust(int i, int j, ref int type)
		{
			return false;
		}
	}
}
