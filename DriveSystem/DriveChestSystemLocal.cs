using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.Audio;

namespace SatelliteStorage.DriveSystem
{
    public class DriveChestSystemLocal
    {
        /// <param name="allowNewItems">false stands for quick stack</param>
        /// <param name="autoImportOnly">to import only those items that are marked as favorites (auto import)</param>
        /// <param name="includeHands">include slots from 0 to 9</param>
        public static bool DepositItemsFromInventory(bool allowNewItems = false, bool autoImportOnly = false, bool includeHands = false)
        {
            Player player = Main.LocalPlayer;
            bool itemAdded = false;
            for (int i = includeHands ? 0 : 10; i < player.inventory.Length; i++)
            {
                Item item = player.inventory[i];

                if (
                    item != null &&
                    !item.favorited &&
                    !item.IsAir &&
                    i != 58 &&
                    (autoImportOnly ? SatelliteStorage.AutoImportItems.ContainsKey(item.type) : true)
                )
                {
                    DriveItem driveItem = new DriveItem();

                    driveItem.type = item.type;
                    driveItem.stack = item.stack;
                    driveItem.prefix = item.prefix;

                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        if ((allowNewItems ? true : DriveChestSystem.HasItem(driveItem)) && DriveChestSystem.AddItem(driveItem))
                        {
                            item.TurnToAir();
                            itemAdded = true;
                        }
                    }

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        ModPacket packet = SatelliteStorage.instance.GetPacket();
                        packet.Write((byte)SatelliteStorage.MessageType.DepositDriveChestItem);
                        packet.Write((byte)player.whoAmI);
                        packet.Write((byte)(allowNewItems ? 1 : 0));
                        packet.Write((byte)i);

                        packet.Send();
                        packet.Close();

                        if ((allowNewItems ? true : DriveChestSystem.HasItem(driveItem)))
                        {
                            itemAdded = true;
                        }
                    }

                }
            }

            return itemAdded;
        }


        public static bool CraftItem(int recipeID)
        {
            Recipe recipe = Main.recipe[recipeID];
            Player player = Main.LocalPlayer;
            Item mouseItem = player.inventory[58];

            bool isMouseItemAir = mouseItem.IsAir && Main.mouseItem.IsAir;
            bool isMouseItemSame = mouseItem.type == recipe.createItem.type;
            if (!isMouseItemAir && !isMouseItemSame) return false;

            if (isMouseItemSame)
            {
                if (mouseItem.stack + recipe.createItem.stack > mouseItem.maxStack) return false;
            }

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                List<RecipeItemsUses> uses = DriveChestSystem.GetItemsUsesForCraft(player.inventory, recipe);
                if (uses == null) return false;
                uses.ForEach(u =>
                {
                    Item item = new Item();
                    item.type = u.type;
                    item.SetDefaults(item.type);
                    item.stack = 1;

                    if (u.from == 0)
                    {
                        player.inventory[u.slot].stack -= u.stack;
                        if (player.inventory[u.slot].stack <= 0) player.inventory[u.slot].TurnToAir();
                    }
                    else
                    {
                        DriveChestSystem.SubItem(u.type, u.stack);
                    }
                });

                if (isMouseItemAir)
                {
                    Main.mouseItem = recipe.createItem.Clone();
                }
                else
                {
                    Main.mouseItem.stack += recipe.createItem.stack;
                }

                return true;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                List<RecipeItemsUses> uses = DriveChestSystem.GetItemsUsesForCraft(player.inventory, recipe);
                if (uses == null) return false;

                ModPacket packet = SatelliteStorage.instance.GetPacket();
                packet.Write((byte)SatelliteStorage.MessageType.TryCraftRecipe);
                packet.Write((byte)player.whoAmI);
                packet.Write7BitEncodedInt(recipeID);
                packet.Send();
                packet.Close();
            }

            return false;
        }
    }
}
