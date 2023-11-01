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
using SatelliteStorage.Generators;
using SatelliteStorage.ModNetwork;

namespace SatelliteStorage.Tiles
{
    class BaseItemsGeneratorTile : ModTile
    {
		public byte generatorType;
		public int itemDrop;
		public string item_name;
		private int animationUniqueFrame = -1;

        public override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileOreFinderPriority[Type] = 500;
			TileID.Sets.DisableSmartCursor[Type] = true;

			LocalizedText name = CreateMapEntryName();
			name.Format(Language.GetTextValue("Mods.SatelliteStorage.Tiles." + item_name+ "Tile"));
			AddMapEntry(new Color(108, 65, 138), name);


			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.Origin = new Point16(1, 1);
			TileObjectData.newTile.CoordinateHeights = new int[3] { 16, 16, 16 };
			TileObjectData.newTile.CoordinateWidth = 16;

			AnimationFrameHeight = 54;

			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = false;

			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);

			TileObjectData.addTile(Type);

			SetGeneratorDefaults();
		}

		public virtual void SetGeneratorDefaults()
        {
			itemDrop = ModContent.ItemType<Items.DriveChestItem>();
			generatorType = (byte)SatelliteStorage.GeneratorTypes.BaseGenerator;
		}

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = 1;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			if (Main.netMode == NetmodeID.SinglePlayer || Main.netMode == NetmodeID.Server)
			{
                SatelliteStorage.generatorsSystem.TakeGeneratorFromInv(generatorType);
			}
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;

			player.cursorItemIconText = Language.GetTextValue("Mods.SatelliteStorage.Tiles." + item_name + "Tile.MapEntry");

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
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
                SatelliteStorage.generatorsSystem.AddGeneratorToInv(generatorType);
			}

			if (Main.netMode == NetmodeID.MultiplayerClient)
            {
				Player player = Main.LocalPlayer;
				ModPacket packet = Mod.GetPacket();
				packet.Write((byte)MessageType.ChangeGeneratorState);
				packet.Write((byte)player.whoAmI);
				
				packet.Write((byte)generatorType);
				packet.Write((byte)1);

				packet.Send();
				packet.Close();
			}

			base.PlaceInWorld(i, j, item);
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Main.tile[i, j];
			Texture2D texture = ModContent.Request<Texture2D>("SatelliteStorage/Tiles/" + Name).Value;
			Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

			int frameYOffset = Main.tileFrame[Type] * AnimationFrameHeight;

			spriteBatch.Draw(
				texture,
				new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
				new Rectangle(tile.TileFrameX, tile.TileFrameY + frameYOffset, 16, 16),
				Lighting.GetColor(i, j), 0f, default, 1f, SpriteEffects.None, 0f);

			

			return false;
		}

		public override void AnimateTile(ref int frame, ref int frameCounter)
		{
			if (++frameCounter >= 10) {
				frameCounter = 0;
				frame = ++frame % 4;
			}
		}
	}
}
