using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace SatelliteStorage.Items
{
    class QuartzSword : ModItem
    {
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 40; // The item texture's width.
			Item.height = 40; // The item texture's height.

			Item.useStyle = ItemUseStyleID.Swing; // The useStyle of the Item.
			Item.useTime = 100;
			Item.useAnimation = 15;
			Item.autoReuse = true; // Whether the weapon can be used more than once automatically by holding the use button.

			Item.DamageType = DamageClass.Melee; // Whether your item is part of the melee class.
			Item.damage = 25; // The damage your item deals.
			Item.knockBack = 5; // The force of knockback of the weapon. Maximum is 20
			Item.crit = 0; // The critical strike chance the weapon has. The player, by default, has a 4% critical strike chance.
			Item.useTurn = false;
			Item.scale = 1.40f;
			Item.value = 500; // The value of the weapon in copper coins.
			Item.rare = ItemRarityID.Purple; // Give this item our custom rarity.
			Item.UseSound = SoundID.Item1; // The sound when the weapon is being used.
			Item.maxStack = 1;
		}


		public override void MeleeEffects(Player player, Rectangle hitbox)
		{
			Vector2 target = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY);

			if (player.position.X > target.X)
			{
				player.direction = -1;
			}
			else
			{
				player.direction = 1;
			}

			if (Main.rand.NextBool(5))
			{
				Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.IceTorch, Scale: 2f, SpeedX: 5 * player.direction);
			}

			if (Main.rand.NextBool(8))
			{
				Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.GemAmethyst, Scale: 1f, SpeedX: 3 * player.direction);
			}
		}



        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
		{
			// Inflict the OnFire debuff for 1 second onto any NPC/Monster that this hits.
			// 60 frames = 1 second
			target.AddBuff(BuffID.AmethystMinecartLeft, 2);
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<Items.QuartzShard>(), 8)
				.AddIngredient(ItemID.Wood, 3)
				.AddIngredient(ItemID.TissueSample, 30)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
