using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace SatelliteStorage.Items
{
    class ItemsCountAccessoryItem : ModItem
    {
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
			Item.rare = ItemRarityID.Blue;
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 34;
			Item.accessory = true;
			Item.defense = 2;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{

		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<Items.QuartzShard>(), 3)
				.AddIngredient(ItemID.Glass, 10)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}

