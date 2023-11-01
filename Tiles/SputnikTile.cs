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
using System;
using System.ComponentModel;
using SatelliteStorage.DriveSystem;
using SatelliteStorage.ModNetwork;

namespace SatelliteStorage.Tiles
{
    class SputnikTile : ModTile
    {

        public override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileOreFinderPriority[Type] = 500;
			TileID.Sets.DisableSmartCursor[Type] = true;

			DustType = DustID.Firefly;
			
			LocalizedText name = CreateMapEntryName();
			name.Format(Language.GetTextValue("Mods.SatelliteStorage.Tiles.SputnikTile"));
			AddMapEntry(new Color(108, 65, 138), name);
			
			TileObjectData.newTile.CopyFrom(TileObjectData.Style6x3);
			TileObjectData.newTile.Origin = new Point16(2, 1);
			TileObjectData.newTile.CoordinateHeights = new int[3] { 16, 16, 16 };
			TileObjectData.newTile.CoordinateWidth = 16;

			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = false;

			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.EmptyTile, TileObjectData.newTile.Width, 0);

			TileObjectData.addTile(Type);
		}

        public override bool CanPlace(int i, int j)
        {
			if (Main.LocalPlayer.position.Y > Main.worldSurface * 4.2) return false;
			if (SatelliteStorage.driveChestSystem.isSputnikPlaced) return false;
            return base.CanPlace(i, j);
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = 1;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
            SatelliteStorage.driveChestSystem.isSputnikPlaced = false;
            SatelliteStorage.driveChestSystem.SyncIsSputnikPlacedToClients();
		}

		private void SendSyncSputnikState()
        {
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				Player player = Main.LocalPlayer;
				ModPacket packet = Mod.GetPacket();
				packet.Write((byte)MessageType.SetSputnikState);
				packet.Write((byte)player.whoAmI);
				packet.Write((byte)(SatelliteStorage.driveChestSystem.isSputnikPlaced ? 1 : 0));
				packet.Send();
				packet.Close();
			}
		}

		public override bool RightClick(int i, int j)
		{

			return true;
		}

		public override void MouseOver(int i, int j)
		{

			Player player = Main.LocalPlayer;


			player.cursorItemIconText = Language.GetTextValue("Mods.SatelliteStorage.UITitles.SputnikItem");

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
            SatelliteStorage.driveChestSystem.isSputnikPlaced = true;
			SendSyncSputnikState();
			base.PlaceInWorld(i, j, item);
		}
	}
}
