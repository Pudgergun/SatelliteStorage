using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.Creative;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace SatelliteStorage.Items
{
    class QuartzCrateItem : ModItem
    {

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}"); 

			ItemID.Sets.IsFishingCrate[Type] = true;

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 10;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.QuartzCrateTile>());
			Item.width = 12;
			Item.height = 12;
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Purple;
			//Item.value = Item.sellPrice(0, 2);
		}

		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
		{
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.Crates;
		}

		public override bool CanRightClick()
		{
			return true;
		}

		public override void ModifyItemLoot(ItemLoot itemLoot)
		{
			// Drop a special weapon/accessory etc. specific to this crate's theme (i.e. Sky Crate dropping Fledgling Wings or Starfury)
			int[] themedDrops = new int[] {
				ModContent.ItemType<Items.DriveChestItem>(),
				ModContent.ItemType<Items.ItemsCountAccessoryItem>()
			};
			itemLoot.Add(ItemDropRule.OneFromOptionsNotScalingWithLuck(50, themedDrops));

			// Drop coins
			itemLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 4, 5, 13));

			// Drop pre-hm ores, with the addition of one from ExampleMod
			IItemDropRule[] oreTypes = new IItemDropRule[] {
				ItemDropRule.Common(ItemID.CopperOre, 1, 30, 50),
				ItemDropRule.Common(ItemID.TinOre, 1, 30, 50),
				ItemDropRule.Common(ItemID.IronOre, 1, 30, 50),
				ItemDropRule.Common(ItemID.LeadOre, 1, 30, 50),
				ItemDropRule.Common(ItemID.SilverOre, 1, 30, 50),
				ItemDropRule.Common(ItemID.TungstenOre, 1, 30, 50),
				ItemDropRule.Common(ItemID.GoldOre, 1, 30, 50),
				ItemDropRule.Common(ItemID.PlatinumOre, 1, 30, 50),
			};
			itemLoot.Add(new OneFromRulesRule(7, oreTypes));

			// Drop pre-hm bars (except copper/tin), with the addition of one from ExampleMod
			IItemDropRule[] oreBars = new IItemDropRule[] {
				ItemDropRule.Common(ItemID.IronBar, 1, 10, 21),
				ItemDropRule.Common(ItemID.LeadBar, 1, 10, 21),
				ItemDropRule.Common(ItemID.SilverBar, 1, 10, 21),
				ItemDropRule.Common(ItemID.TungstenBar, 1, 10, 21),
				ItemDropRule.Common(ItemID.GoldBar, 1, 10, 21),
				ItemDropRule.Common(ItemID.PlatinumBar, 1, 10, 21),
				ItemDropRule.Common(ModContent.ItemType<Items.QuartzShard>(), 1, 5, 10),
				ItemDropRule.Common(ModContent.ItemType<Items.QuartzShard>(), 1, 5, 10),
				ItemDropRule.Common(ModContent.ItemType<Items.QuartzShard>(), 1, 5, 10),
			};

			itemLoot.Add(new OneFromRulesRule(4, oreBars));

			// Drop an "exploration utility" potion, with the addition of one from ExampleMod
			IItemDropRule[] explorationPotions = new IItemDropRule[] {
				ItemDropRule.Common(ItemID.ObsidianSkinPotion, 1, 2, 5),
				ItemDropRule.Common(ItemID.SpelunkerPotion, 1, 2, 5),
				ItemDropRule.Common(ItemID.HunterPotion, 1, 2, 5),
				ItemDropRule.Common(ItemID.GravitationPotion, 1, 2, 5),
				ItemDropRule.Common(ItemID.MiningPotion, 1, 2, 5),
				ItemDropRule.Common(ItemID.HeartreachPotion, 1, 2, 5),
			};
			itemLoot.Add(new OneFromRulesRule(4, explorationPotions));

			// Drop (pre-hm) resource potion
			IItemDropRule[] resourcePotions = new IItemDropRule[] {
				ItemDropRule.Common(ItemID.HealingPotion, 1, 5, 18),
				ItemDropRule.Common(ItemID.ManaPotion, 1, 5, 18),
			};
			itemLoot.Add(new OneFromRulesRule(2, resourcePotions));

			// Drop (high-end) bait
			IItemDropRule[] highendBait = new IItemDropRule[] {
				ItemDropRule.Common(ItemID.JourneymanBait, 1, 2, 7),
				ItemDropRule.Common(ItemID.MasterBait, 1, 2, 7),
			};
			itemLoot.Add(new OneFromRulesRule(2, highendBait));
		}
	}
}
