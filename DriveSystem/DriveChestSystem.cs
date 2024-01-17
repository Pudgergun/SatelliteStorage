using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;
using System;
using Terraria.Audio;
using SatelliteStorage.UI;
using Microsoft.Xna.Framework;
using SatelliteStorage.ModNetwork;

namespace SatelliteStorage.DriveSystem
{
    public class DriveChestSystem
    {
        private List<IDriveItem> _items = new List<IDriveItem>();
		private Dictionary<int, Recipe> _availableRecipes = new Dictionary<int, Recipe>();

        public bool isSputnikPlaced = false;

        public bool checkRecipesRefresh = false;
        public int itemsCount { get; private set; } = 0;
		private bool Debug_fillWithRandomItems = false;

		private Mod _mod;
		private UIDriveChest _driveChestUI;

		public event Action<bool, Vector2> OnDriveChestOpened;

        public bool addDriveChestItemSended;
        public bool takeDriveChestItemSended;

        public DriveChestSystem(Mod mod, UIDriveChest driveChestUI)
		{
			_mod = mod;
            _driveChestUI = driveChestUI;
        }

        public void InitItems(List<IDriveItem> items)
        {
            checkRecipesRefresh = false;
            _items = items;

			//========= DEBUG
			if (Debug_fillWithRandomItems)
			{
				Random rnd = new Random();
				for (int i = 0; i < 10000; i++)
				{
                    IDriveItem itm = new DriveItem();
					itm.SetType(i + 1).SetStack(1).SetPrefix(rnd.Next(1, 30));
                    _items.Add(itm);
				}
			}
			//===========

			foreach(IDriveItem itm in items)
            {
				if (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.SinglePlayer) itemsCount += itm.stack;
			}

			SendSyncItemsCount();
		}

        public void ClearItems()
        {
            _items.Clear();
			if (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.SinglePlayer) itemsCount = 0;
		}

		public void SetItemsCount(int count)
		{
			itemsCount = count;
		}

        public List<IDriveItem> GetItems() => _items;
        public Dictionary<int, Recipe> GetAvailableRecipes() => _availableRecipes;

        public bool AddItem(IDriveItem item, bool needSync = true)
        {
            IDriveItem searchItem = _items.Find(v => v.type == item.type && v.prefix == item.prefix);
            if (searchItem != null)
            {
                searchItem.AddStack(item.stack);
				itemsCount += item.stack;
				if (needSync) SendItemSync(searchItem);
				if (needSync) SendSyncItemsCount();
				return true;
            }

            _items.Add(item);
			itemsCount += item.stack;

			if (needSync) SendSyncItemsCount();
			if (needSync) SendItemSync(item);

            return true;
        }

		public void SyncItemByType(int type)
        {
            IDriveItem searchItem = _items.Find(v => v.type == type);
			if (searchItem == null) return;
			SendItemSync(searchItem);
		}

        public bool HasItem(IDriveItem item)
        {
            IDriveItem searchItem = _items.Find(v => v.type == item.type && v.prefix == item.prefix);
            if (searchItem != null) return true;
            return false;
        }

		public int GetItemCount(IDriveItem item)
		{
            IDriveItem searchItem = _items.Find(v => v.type == item.type && v.prefix == item.prefix);
			if (searchItem != null) return searchItem.stack;
			return 0;
		}

		public void SyncItem(IDriveItem item)
        {
            IDriveItem searchItem = _items.Find(v => v.type == item.type && v.prefix == item.prefix);
            if (searchItem != null)
            {
                searchItem.SetStack(item.stack);
                if (searchItem.stack <= 0) _items.Remove(searchItem);
            }
            else
            {
                _items.Add(item);
            }

            checkRecipesRefresh = false;
        }

		public void SendSyncItemsCount(int to = -1)
        {
			if (Main.netMode != NetmodeID.Server)
			{
				return;
			}

			var packet = _mod.GetPacket();
			packet.Write((byte)MessageType.StorageItemsCount);
			packet.Write7BitEncodedInt(itemsCount);
			packet.Send(to);
			packet.Close();
		}

        public void SyncIsSputnikPlacedToClients()
        {
            if (Main.netMode != NetmodeID.Server) return;
            var spPacket = _mod.GetPacket();
            spPacket.Write((byte)MessageType.SetSputnikState);
            spPacket.Write((byte)(isSputnikPlaced ? 1 : 0));
            spPacket.Send(-1);
            spPacket.Close();
        }

        public void SendSputnikState(int playernumber)
        {
            var rsPacket = _mod.GetPacket();
            rsPacket.Write((byte)MessageType.SetSputnikState);
            rsPacket.Write((byte)(isSputnikPlaced ? 1 : 0));
            rsPacket.Send(playernumber);
            rsPacket.Close();
        }

		public void CallDriveChestOpened(bool positionChecking, Vector2 position)
		{
			OnDriveChestOpened.Invoke(positionChecking, position);
		}

		public void AddItemFromMouse()
		{
            Player player = Main.LocalPlayer;
            Item mouseItem = player.inventory[58];

            if (mouseItem.IsAir || Main.mouseItem.IsAir) return;


            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                if (!AddItem(DriveItem.FromItem(Main.mouseItem))) return;
                _driveChestUI.ReloadItems();

                mouseItem.TurnToAir();
                Main.mouseItem.TurnToAir();
                SoundEngine.PlaySound(SoundID.Grab);
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (addDriveChestItemSended) return;
                addDriveChestItemSended = true;
                ModPacket packet = _mod.GetPacket();
                packet.Write((byte)MessageType.AddDriveChestItem);
                packet.Write((byte)player.whoAmI);
                packet.Write((byte)0);
                packet.Send();
                packet.Close();
            }
        }

		public void TakeItemClicked(IDriveItem clickedItem, int clickType)
		{
            Item hoverItem = clickedItem.ToItem();

            Player player = Main.LocalPlayer;
            Item mouseItem = player.inventory[58];
            int slotToAdd = -1;
            int countToAdd = 0;

            bool isMouseItemAir = mouseItem.IsAir && Main.mouseItem.IsAir;
            bool isMouseItemSame = mouseItem.type == clickedItem.type;
            if (!isMouseItemAir && !isMouseItemSame) return;

            if (clickType == 1)
            {
                if (!isMouseItemAir && !isMouseItemSame) return;

                if (isMouseItemSame)
                {
                    if (mouseItem.stack + 1 > mouseItem.maxStack) return;
                }
            }
            else if (clickType == 2)
            {

                if (!isMouseItemAir) return;

                int stackDiff = 0;

                for (int s = 0; s < 50; s++)
                {
                    if (player.inventory[s].type != ItemID.None &&
                        player.inventory[s].type == hoverItem.type &&
                        player.inventory[s].stack < hoverItem.maxStack &&
                        hoverItem.maxStack - player.inventory[s].stack > stackDiff &&
                        !player.inventory[s].favorited
                    )
                    {
                        stackDiff = hoverItem.maxStack - player.inventory[s].stack;
                        countToAdd = stackDiff;
                        if (countToAdd > hoverItem.stack) countToAdd = hoverItem.stack;
                        slotToAdd = s;
                    }
                }

                if (slotToAdd <= -1)
                {
                    for (int s = 0; s < 50; s++)
                    {
                        if (player.inventory[s].type == ItemID.None)
                        {
                            slotToAdd = s;
                            countToAdd = hoverItem.maxStack;
                            if (countToAdd > hoverItem.stack) countToAdd = hoverItem.stack;
                            break;
                        }
                    }
                }
            }
            else
            {
                if (!isMouseItemAir) return;
            }

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                if (clickType == 2 && slotToAdd <= -1) return;

                int takeCount = clickType == 1 ? 1 : 0;
                if (clickType == 2) takeCount = countToAdd;

                Item takeItem = TakeItem(clickedItem.type, clickedItem.prefix, takeCount);
                if (takeItem == null) return;

                if (clickType == 1)
                {
                    if (isMouseItemAir)
                    {
                        Main.mouseItem = takeItem.Clone();
                    }
                    else
                    {
                        Main.mouseItem.stack += 1;
                    }
                }
                else if (clickType == 2)
                {
                    if (player.inventory[slotToAdd].type == ItemID.None)
                    {
                        player.inventory[slotToAdd] = takeItem.Clone();
                    }
                    else
                    {
                        player.inventory[slotToAdd].stack += countToAdd;
                    }

                    SoundEngine.PlaySound(SoundID.Grab);
                }
                else
                {
                    Main.mouseItem = takeItem;
                }

                _driveChestUI.ReloadItems();

                if (clickType == 1)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.Grab);
                }
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (takeDriveChestItemSended) return;
                takeDriveChestItemSended = true;

                ModPacket packet = _mod.GetPacket();
                packet.Write((byte)MessageType.TakeDriveChestItem);
                packet.Write((byte)player.whoAmI);
                packet.Write((byte)clickType);
                packet.Write7BitEncodedInt(clickedItem.type);
                packet.Write7BitEncodedInt(clickedItem.prefix);
                packet.Send();
                packet.Close();
            }
        }

        public bool InventorySlotShift(Item[] inventory, int context, int slot)
        {
            if (_driveChestUI.isDrawing)
            {
                if (Main.LocalPlayer.inventory[slot].IsAir) return false;
                if (Main.LocalPlayer.inventory[slot].favorited) return false;

                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    if (!AddItem(DriveItem.FromItem(Main.LocalPlayer.inventory[slot]))) return false;
                    _driveChestUI.ReloadItems();
                    Main.LocalPlayer.inventory[slot].TurnToAir();
                    SoundEngine.PlaySound(SoundID.Grab);
                }

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    if (addDriveChestItemSended) return false;
                    addDriveChestItemSended = true;
                    ModPacket packet = _mod.GetPacket();
                    packet.Write((byte)MessageType.AddDriveChestItem);
                    packet.Write((byte)Main.LocalPlayer.whoAmI);
                    packet.Write((byte)1);
                    packet.Write7BitEncodedInt(slot);
                    packet.Send();
                    packet.Close();
                }

                return false;
            }

            return true;
        }

        /// <param name="allowNewItems">false stands for quick stack</param>
        /// <param name="autoImportOnly">to import only those items that are marked as favorites (auto import)</param>
        /// <param name="includeHands">include slots from 0 to 9</param>
        public bool DepositItemsFromInventory(bool allowNewItems = false, bool autoImportOnly = false, bool includeHands = false)
        {
            Player player = Main.LocalPlayer;
            bool itemAdded = false;

            SatelliteStoragePlayer modPlayer = Main.LocalPlayer.GetModPlayer<SatelliteStoragePlayer>();

            for (int i = includeHands ? 0 : 10; i < player.inventory.Length; i++)
            {
                Item item = player.inventory[i];

                if (
                    item != null &&
                    !item.favorited &&
                    !item.IsAir &&
                    i != 58 &&
                    (autoImportOnly ? modPlayer.GetAutoImportItems().ContainsKey(item.type) : true)
                )
                {
                    IDriveItem driveItem = new DriveItem()
						.SetType(item.type)
						.SetStack(item.stack)
						.SetPrefix(item.prefix);


                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        if ((allowNewItems ? true : HasItem(driveItem)) && AddItem(driveItem))
                        {
                            item.TurnToAir();
                            itemAdded = true;
                        }
                    }

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        ModPacket packet = _mod.GetPacket();
                        packet.Write((byte)MessageType.DepositDriveChestItem);
                        packet.Write((byte)player.whoAmI);
                        packet.Write((byte)(allowNewItems ? 1 : 0));
                        packet.Write((byte)i);

                        packet.Send();
                        packet.Close();

                        if ((allowNewItems ? true : HasItem(driveItem)))
                        {
                            itemAdded = true;
                        }
                    }

                }
            }

            return itemAdded;
        }

        public bool CraftItem(int recipeID)
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
                List<RecipeItemStruct> uses = GetItemsUsesForCraft(player.inventory, recipe);
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
                        SubItem(u.type, u.stack);
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
                List<RecipeItemStruct> uses = GetItemsUsesForCraft(player.inventory, recipe);
                if (uses == null) return false;

                ModPacket packet = _mod.GetPacket();
                packet.Write((byte)MessageType.TryCraftRecipe);
                packet.Write((byte)player.whoAmI);
                packet.Write7BitEncodedInt(recipeID);
                packet.Send();
                packet.Close();
            }

            return false;
        }

        private void SendItemSync(IDriveItem item)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                checkRecipesRefresh = false;
                return;
            }

            var packet = _mod.GetPacket();
            packet.Write((byte)MessageType.SyncDriveChestItem);
            packet.Write7BitEncodedInt(item.type);
            packet.Write7BitEncodedInt(item.stack);
            packet.Write7BitEncodedInt(item.prefix);
            packet.Send(-1);
            packet.Close();
        }

        public Item TakeItem(int type, int prefix, int count = 0)
        {
            IDriveItem searchItem = _items.Find(v => v.type == type && v.prefix == prefix);
            if (searchItem == null) return null;
            Item item = searchItem.ToItem();

            int stack = searchItem.stack;
			if (count > 0) stack = count;
			if (stack > item.maxStack) stack = item.maxStack;
			

            item.stack = stack;

			int oldStackCount = searchItem.stack + 0;

            searchItem.SubStack(stack);

			int takeItemsCount = stack;

			if (searchItem.stack <= 0)
			{
				takeItemsCount = oldStackCount;
				_items.Remove(searchItem);
			}

			if (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.SinglePlayer) itemsCount -= takeItemsCount;
			SendSyncItemsCount();

			SendItemSync(searchItem);

            return item;
        }

		public void SubItem(int type, int count)
		{
            IDriveItem searchItem = _items.Find(v => v.type == type);
			if (searchItem == null) return;

			int oldStackCount = searchItem.stack + 0;

			searchItem.SubStack(count);

			int takeItemsCount = count;

			if (searchItem.stack <= 0)
			{
				takeItemsCount = oldStackCount;
				_items.Remove(searchItem);
			}

			if (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.SinglePlayer) itemsCount -= takeItemsCount;
			SendSyncItemsCount();

			SendItemSync(searchItem);
		}


        public List<RecipeItemStruct> GetItemsUsesForCraft(Item[] playerInv, Recipe recipe)
		{
			List<RecipeItemStruct> uses = new List<RecipeItemStruct>();
			Dictionary<int, int> recipeItems = new Dictionary<int, int>();
			Dictionary<int, int> hasItems = new Dictionary<int, int>();

			recipe.requiredItem.ForEach(r =>
			{
				recipeItems[r.type] = r.stack;
				hasItems[r.type] = 0;
			});

			
			for (int l = 0; l < 58; l++)
			{
				Item invItem = playerInv[l];
				if (recipeItems.ContainsKey(invItem.type))
                {
					if (hasItems[invItem.type] < recipeItems[invItem.type])
					{
						hasItems[invItem.type] += invItem.stack;
						RecipeItemStruct usesItem = new RecipeItemStruct();
						usesItem.type = invItem.type;
						usesItem.stack = invItem.stack;

						if (hasItems[invItem.type] > recipeItems[invItem.type]) hasItems[invItem.type] = recipeItems[invItem.type];
						if (usesItem.stack > recipeItems[usesItem.type]) usesItem.stack = recipeItems[usesItem.type];

						usesItem.slot = l;
						usesItem.from = 0;

						if (usesItem.stack > 0) uses.Add(usesItem);
					}
				}
			}
			

			for (int i = 0; i < _items.Count; i++)
			{
                IDriveItem driveItem = _items[i];
				if (recipeItems.ContainsKey(driveItem.type))
                {
					if (hasItems[driveItem.type] < recipeItems[driveItem.type])
                    {
						int needItems = recipeItems[driveItem.type] - hasItems[driveItem.type];
						RecipeItemStruct usesItem = new RecipeItemStruct();
						usesItem.type = driveItem.type;
						usesItem.stack = driveItem.stack;
						usesItem.from = 1;
						hasItems[driveItem.type] += driveItem.stack;
						if (hasItems[driveItem.type] > recipeItems[driveItem.type]) hasItems[driveItem.type] = recipeItems[driveItem.type];
						if (usesItem.stack > needItems) usesItem.stack = needItems;
						uses.Add(usesItem);
					}
				}
			}

			foreach(int key in recipeItems.Keys)
            {
				if (hasItems[key] < recipeItems[key]) return null;
            }

			return uses;
		}

        public bool ToggleDriveChestMenu(bool checkPosition = false)
        {
			Player player = Main.LocalPlayer;
			Main.mouseRightRelease = false;

            if (_driveChestUI.GetState())
			{
				_driveChestUI.SetState(false);

                SoundEngine.PlaySound(SoundID.MenuClose);
				return true;
			}

			if (player.sign >= 0)
			{
				SoundEngine.PlaySound(SoundID.MenuClose);
				player.sign = -1;
				Main.editSign = false;
				Main.npcChatText = "";
			}

			if (Main.editChest)
			{
				SoundEngine.PlaySound(SoundID.MenuTick);
				Main.editChest = false;
				Main.npcChatText = "";
			}

			if (player.editedChestName)
			{
				player.editedChestName = false;
			}

			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				SoundEngine.PlaySound(SoundID.MenuOpen);
				Main.playerInventory = true;
                _driveChestUI.SetState(true);

				CallDriveChestOpened(checkPosition, Main.LocalPlayer.position);
			}

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				ModPacket packet = _mod.GetPacket();
				packet.Write((byte)MessageType.RequestDriveChestItems);
				packet.Write((byte)player.whoAmI);
				packet.Write((byte)(checkPosition ? 1: 0));

				packet.Send();
				packet.Close();
			}

			return true;
		}



        public void ResearchRecipes()
        {
			Player player = Main.LocalPlayer;
			Item mouseItem = player.inventory[58];
			int mouseItemType = -1;
			if (!mouseItem.IsAir && !Main.mouseItem.IsAir) mouseItemType = mouseItem.type;

			int maxRecipes = Recipe.maxRecipes;

			_availableRecipes.Clear();

			Dictionary<int, int> dictionary = new Dictionary<int, int>();
            Item[] array = Main.player[Main.myPlayer].inventory;

            Item item;
            for (int l = 0; l < 58; l++)
            {
                item = array[l];
                if (item.stack > 0)
                {
                    if (dictionary.ContainsKey(item.type))
                    {
                        dictionary[item.type] += item.stack;
                    }
                    else
                    {
                        dictionary[item.type] = item.stack;
                    }
                }
            }

            List<IDriveItem> driveItems = GetItems();

			for (int l = 0; l < driveItems.Count; l++)
			{
                IDriveItem driveItem = driveItems[l];

				if (dictionary.ContainsKey(driveItem.type))
				{
					dictionary[driveItem.type] += driveItem.stack;
				}
				else
				{
					dictionary[driveItem.type] = driveItem.stack;
				}
			}

			for (int n = 0; n < maxRecipes && Main.recipe[n].createItem.type != 0; n++)
			{
				bool flag = true;
				if (flag)
				{
					for (int num3 = 0; num3 < Main.recipe[n].requiredTile.Count && Main.recipe[n].requiredTile[num3] != -1; num3++)
					{
						if (!Main.player[Main.myPlayer].adjTile[Main.recipe[n].requiredTile[num3]])
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					for (int num4 = 0; num4 < Main.recipe[n].requiredItem.Count; num4++)
					{
						item = Main.recipe[n].requiredItem[num4];
						if (item.type == 0)
						{
							break;
						}
						int num5 = item.stack;
						bool flag2 = false;

						if (!flag2 && dictionary.ContainsKey(item.type))
						{
							num5 -= dictionary[item.type];
						}
						if (num5 > 0)
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
                    bool num6 = !Main.recipe[n].HasCondition(Condition.NearWater) || Main.player[Main.myPlayer].adjWater || Main.player[Main.myPlayer].adjTile[172];
					bool flag3 = !Main.recipe[n].HasCondition(Condition.NearHoney) || Main.recipe[n].HasCondition(Condition.NearHoney) == Main.player[Main.myPlayer].adjHoney;
					bool flag4 = !Main.recipe[n].HasCondition(Condition.NearLava) || Main.recipe[n].HasCondition(Condition.NearLava) == Main.player[Main.myPlayer].adjLava;
					bool flag5 = !Main.recipe[n].HasCondition(Condition.InSnow) || Main.player[Main.myPlayer].ZoneSnow;
					bool flag6 = !Main.recipe[n].HasCondition(Condition.InGraveyard) || Main.player[Main.myPlayer].ZoneGraveyard;
					if (!(num6 && flag3 && flag4 && flag5 && flag6))
					{
						flag = false;
					}
				}
				if (flag)
				{
					if (mouseItemType > -1 && Main.recipe[n].createItem.type == mouseItemType)
                    {

					}

					_availableRecipes[n] = Main.recipe[n];
				}
			}
		}
    }
}
