﻿using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;
using System;
using Terraria.Audio;

namespace SatelliteStorage.DriveSystem
{
	public class RecipeItemsUses
    {
		public int type = 0;
		public int stack = 0;
		public int from = 0;
		public int slot = 0;
    }

    public class DriveChestSystem
    {
        public static DriveChestSystem instance;
        private List<DriveItem> items = new List<DriveItem>();
		public Dictionary<int, int> generators = new Dictionary<int, int>();
		public static Dictionary<int, Recipe> availableRecipes = new Dictionary<int, Recipe>();
		private Dictionary<int, int> generatedItemsQueue = new Dictionary<int, int>();

		public static bool isSputnikPlaced = false;

        public static bool checkRecipesRefresh = false;
		public static int itemsCount = 0;

        public DriveChestSystem()
        {
            instance = this;
		}

        public static void InitItems(List<DriveItem> items)
        {
            checkRecipesRefresh = false;
            instance.items = items;

			foreach(DriveItem itm in items)
            {
				if (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.SinglePlayer) itemsCount += itm.stack;
			}

			SendSyncItemsCount();
		}

        public static void ClearItems()
        {
            instance.items.Clear();
			if (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.SinglePlayer) itemsCount = 0;
		}

		public static void ClearGenerators()
        {
			instance.generators.Clear();
		}

        public static List<DriveItem> GetItems()
        {
            return instance.items;
        }

		public static void InitGenerators(Dictionary<int, int> generators)
		{
			instance.generators = generators;
		}

		public static Dictionary<int, int> GetGenerators()
        {
			return instance.generators;
        }

		public static void AddGenerator(byte type)
        {
			if (instance.generators.ContainsKey(type)) instance.generators[type]++;
			else instance.generators[type] = 1;
			SyncGenerator(type);
		}

		public static void SubGenerator(byte type)
        {
			if (!instance.generators.ContainsKey(type)) return;
			instance.generators[type]--;
			if (instance.generators[type] < 0) instance.generators[type] = 0;
			SyncGenerator(type);
		}

		public static void SyncGenerator(byte type, int to = -1)
        {
			if (Main.netMode != NetmodeID.Server) return;

			var packet = SatelliteStorage.instance.GetPacket();
			packet.Write((byte)SatelliteStorage.MessageType.SyncGeneratorState);
			packet.Write((byte)type);
			packet.Write7BitEncodedInt(instance.generators[type]);
			packet.Send(to);
			packet.Close();
		}

		public static void SyncAllGeneratorsTo(int to)
        {
			foreach (byte key in instance.generators.Keys)
            {
				SyncGenerator(key, to);
            }
        }

		public static bool AddItem(DriveItem item, bool needSync = true)
        {
            DriveItem searchItem = instance.items.Find(v => v.type == item.type && v.prefix == item.prefix);
            if (searchItem != null)
            {
                searchItem.stack += item.stack;
				itemsCount += item.stack;
				if (needSync) instance.SendItemSync(searchItem);
				if (needSync) SendSyncItemsCount();
				return true;
            }

            instance.items.Add(item);
			itemsCount += item.stack;

			if (needSync) SendSyncItemsCount();
			if (needSync) instance.SendItemSync(item);

            return true;
        }

		public static void SyncItemByType(int type)
        {
			DriveItem searchItem = instance.items.Find(v => v.type == type);
			if (searchItem == null) return;
			instance.SendItemSync(searchItem);
		}

        public static bool HasItem(DriveItem item)
        {
            DriveItem searchItem = instance.items.Find(v => v.type == item.type && v.prefix == item.prefix);
            if (searchItem != null) return true;
            return false;
        }

		public static int GetItemCount(DriveItem item)
		{
			DriveItem searchItem = instance.items.Find(v => v.type == item.type && v.prefix == item.prefix);
			if (searchItem != null) return searchItem.stack;
			return 0;
		}

		public static void SyncItem(DriveItem item)
        {
            DriveItem searchItem = instance.items.Find(v => v.type == item.type && v.prefix == item.prefix);
            if (searchItem != null)
            {
                searchItem.stack = item.stack;
                if (searchItem.stack <= 0) instance.items.Remove(searchItem);
            }
            else
            {
                instance.items.Add(item);
            }

            checkRecipesRefresh = false;
        }

		public static void SendSyncItemsCount(int to = -1)
        {
			if (Main.netMode != NetmodeID.Server)
			{
				return;
			}

			var packet = SatelliteStorage.instance.GetPacket();
			packet.Write((byte)SatelliteStorage.MessageType.StorageItemsCount);
			packet.Write7BitEncodedInt(itemsCount);
			packet.Send(to);
			packet.Close();
		}
       
        private void SendItemSync(DriveItem item)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                checkRecipesRefresh = false;
                return;
            }

            var packet = SatelliteStorage.instance.GetPacket();
            packet.Write((byte)SatelliteStorage.MessageType.SyncDriveChestItem);
            packet.Write7BitEncodedInt(item.type);
            packet.Write7BitEncodedInt(item.stack);
            packet.Write7BitEncodedInt(item.prefix);
            packet.Send(-1);
            packet.Close();
        }

        public static Item TakeItem(int type, int prefix, int count = 0)
        {
            DriveItem searchItem = instance.items.Find(v => v.type == type && v.prefix == prefix);
            if (searchItem == null) return null;
            Item item = new Item();
            item.type = searchItem.type;
            item.SetDefaults(item.type);
            item.prefix = searchItem.prefix;

            int stack = searchItem.stack;
			if (count > 0) stack = count;
			if (stack > item.maxStack) stack = item.maxStack;
			

            item.stack = stack;

			int oldStackCount = searchItem.stack + 0;

            searchItem.stack -= stack;

			int takeItemsCount = stack;

			if (searchItem.stack <= 0)
			{
				takeItemsCount = oldStackCount;
				instance.items.Remove(searchItem);
			}

			if (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.SinglePlayer) itemsCount -= takeItemsCount;
			SendSyncItemsCount();

			instance.SendItemSync(searchItem);

            return item;
        }

		public static void SubItem(int type, int count)
		{
			DriveItem searchItem = instance.items.Find(v => v.type == type);
			if (searchItem == null) return;

			int oldStackCount = searchItem.stack + 0;

			searchItem.stack -= count;

			int takeItemsCount = count;

			if (searchItem.stack <= 0)
			{
				takeItemsCount = oldStackCount;
				instance.items.Remove(searchItem);
			}

			if (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.SinglePlayer) itemsCount -= takeItemsCount;
			SendSyncItemsCount();

			instance.SendItemSync(searchItem);
		}


		public static List<RecipeItemsUses> GetItemsUsesForCraft(Item[] playerInv, Recipe recipe)
		{
			List<RecipeItemsUses> uses = new List<RecipeItemsUses>();
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
						RecipeItemsUses usesItem = new RecipeItemsUses();
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
			

			for (int i = 0; i < instance.items.Count; i++)
			{
				DriveItem driveItem = instance.items[i];
				if (recipeItems.ContainsKey(driveItem.type))
                {
					if (hasItems[driveItem.type] < recipeItems[driveItem.type])
                    {
						int needItems = recipeItems[driveItem.type] - hasItems[driveItem.type];
						RecipeItemsUses usesItem = new RecipeItemsUses();
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

		public static bool RequestOpenDriveChest(bool checkPosition = false)
        {
			Player player = Main.LocalPlayer;
			Main.mouseRightRelease = false;

			if (SatelliteStorage.GetUIState((int)UI.UITypes.DriveChest))
			{
				SatelliteStorage.SetUIState((int)UI.UITypes.DriveChest, false);
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
				SatelliteStorage.SetUIState((int)UI.UITypes.DriveChest, true);
				if (checkPosition) UI.DriveChestUI.SetOpenedPosition(Main.LocalPlayer.position);
			}

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				ModPacket packet = SatelliteStorage.instance.GetPacket();
				packet.Write((byte)SatelliteStorage.MessageType.RequestDriveChestItems);
				packet.Write((byte)player.whoAmI);
				packet.Write((byte)(checkPosition ? 1: 0));

				packet.Send();
				packet.Close();
			}

			return true;
		}

		public static void OnGeneratorsTick()
        {
			if (Main.netMode != NetmodeID.SinglePlayer && Main.netMode != NetmodeID.Server) return;

			Dictionary<int, int> drops = new Dictionary<int, int>();

			Random random = new Random();

			foreach (int key in instance.generators.Keys)
            {
				for (int i = 0; i < instance.generators[key]; i++)
				{
					Generator generator = SatelliteStorage.instance.generators[key];

					if (random.Next(0, 100) <= generator.chance)
					{
						int index = generator.GetRandomDropIndex();
						int[] data = generator.GetDropData(index);


						if (instance.generatedItemsQueue.ContainsKey(data[0])) instance.generatedItemsQueue[data[0]] += data[1];
						else instance.generatedItemsQueue.Add(data[0], data[1]);
					}
				}
            }

			foreach (int key in instance.generatedItemsQueue.Keys)
			{
				DriveItem addItem = new DriveItem();
				addItem.type = key;
				addItem.stack = instance.generatedItemsQueue[key];
				AddItem(addItem);
			}

			instance.generatedItemsQueue.Clear();
		}

        public static void ResearchRecipes()
        {
			Player player = Main.LocalPlayer;
			Item mouseItem = player.inventory[58];
			int mouseItemType = -1;
			if (!mouseItem.IsAir && !Main.mouseItem.IsAir) mouseItemType = mouseItem.type;

			int maxRecipes = Recipe.maxRecipes;

			availableRecipes.Clear();

			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			Item[] array = null;
			Item item = null;
			array = Main.player[Main.myPlayer].inventory;


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

            List<DriveItem> driveItems = GetItems();

			for (int l = 0; l < driveItems.Count; l++)
			{
				DriveItem driveItem = driveItems[l];

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

					availableRecipes[n] = Main.recipe[n];
				}
			}
		}
    }
}
