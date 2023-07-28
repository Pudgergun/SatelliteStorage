using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using System;

namespace SatelliteStorage.Tiles
{
	public class QuartzOre : ModTile
	{
		public override void SetStaticDefaults()
		{
			TileID.Sets.Ore[Type] = true;
			Main.tileSpelunker[Type] = true;
			Main.tileOreFinderPriority[Type] = 410;
			Main.tileShine2[Type] = true;
			Main.tileShine[Type] = 975;
			Main.tileMergeDirt[Type] = true;
			Main.tileMerge[ItemID.StoneBlock][Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("QuartzOre");
			AddMapEntry(new Color(152, 171, 198), name);

			DustType = 84;
			ItemDrop = ModContent.ItemType<Items.QuartzShard>();

			HitSound = SoundID.Tink;
		}
	}

	public class ExampleOreSystem : ModSystem
	{
		/*
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
		{
			int ShiniesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));

			if (ShiniesIndex != -1)
			{
				tasks.Insert(ShiniesIndex + 1, new QuartzOrePass("Quartz Ore", 237.4298f));
			}
		}
		*/
	}
	/*
	public class QuartzOrePass : GenPass
	{
		public QuartzOrePass(string name, float loadWeight) : base(name, loadWeight)
		{
		}


		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Quartz Ore";

			for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 6E-05); k++)
			{
				int x = WorldGen.genRand.Next(0, Main.maxTilesX);
				int y = WorldGen.genRand.Next((int)WorldGen.worldSurfaceLow, Main.maxTilesY);

				WorldGen.TileRunner(x, y, WorldGen.genRand.Next(3, 6), WorldGen.genRand.Next(2, 6), ModContent.TileType<QuartzOre>());
			}
		}
	}
	*/
}
