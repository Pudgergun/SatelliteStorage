using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.Localization;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using SatelliteStorage.DriveSystem;

namespace SatelliteStorage.Tiles
{
	class DriveChestTile : ModTile
	{

        public override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileShine2[Type] = true;
			Main.tileShine[Type] = 1200;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileOreFinderPriority[Type] = 500;
			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			DustType = DustID.BlueCrystalShard;

			LocalizedText name = CreateMapEntryName();
			name.Format(Language.GetTextValue("Mods.SatelliteStorage.Tiles.DriveChestTile"));
			AddMapEntry(new Color(73, 137, 201), name);

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
			TileObjectData.newTile.Origin = new Point16(2, 1);
			TileObjectData.newTile.CoordinateHeights = new int[3] { 16, 16, 16 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = false;

			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.addTile(Type);
		}

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = 1;
		}


		public override bool RightClick(int i, int j)
		{
			if (!SatelliteStorage.driveChestSystem.isSputnikPlaced)
			{
				Main.NewText(Language.GetTextValue("Mods.SatelliteStorage.Common.CantUseWithoutSputnik"), new Color(173, 57, 71));
				return true;
			}
			return SatelliteStorage.driveChestSystem.ToggleDriveChestMenu(true);
		}

		public override void MouseOver(int i, int j)
		{

			Player player = Main.LocalPlayer;


			player.cursorItemIconText = Language.GetTextValue("Mods.SatelliteStorage.UITitles.DriveChest");

			player.noThrow = 2;
		}

		public override void MouseOverFar(int i, int j)
		{
			MouseOver(i, j);
			Player player = Main.LocalPlayer;
			if (player.cursorItemIconText == "")
			{
				player.cursorItemIconEnabled = false;
				player.cursorItemIconID = 0;
			}
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameX == 0)
			{
				r = 1f;
				g = 0.75f;
				b = 1f;
			}
		}

        public override void PlaceInWorld(int i, int j, Item item)
        {
			if (Main.netMode == NetmodeID.Server)
            {
            }

            base.PlaceInWorld(i, j, item);
        }
    }
}
