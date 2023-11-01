using Terraria.ModLoader;
using Terraria;
using System.Collections.Generic;
using Terraria.ModLoader.IO;
using Terraria.ID;
using SatelliteStorage.DriveSystem;
using SatelliteStorage.UI;
using Terraria.Audio;
using SatelliteStorage.Utils;
using SatelliteStorage.ModNetwork;

namespace SatelliteStorage
{
    class SatelliteStoragePlayer : ModPlayer
    {
        private static List<bool> oldAdjList;
        private static double longTickMilliseconds;
        public Dictionary<int, IDriveItem> _autoImportItems = new Dictionary<int, IDriveItem>();

        public int TempTakeItemCount;
        public int TempTakeItemType;

        public int TempCraftItemCount;
        public int TempCraftItemType;

        public bool showItemsCount { get; private set; }
        public bool hasDriveRemoteItem { get; private set; }

        public Dictionary<int, IDriveItem> GetAutoImportItems() => _autoImportItems;
        public void ClearAutoImportItems() => _autoImportItems.Clear();
        public void ToggleItemAutoImportClicked(IDriveItem item)
        {
            if (_autoImportItems.ContainsKey(item.type))
                _autoImportItems.Remove(item.type);
            else
                _autoImportItems.Add(item.type, item);
        }

        public override void Load()
        {
            longTickMilliseconds = Main.gameTimeCache.TotalGameTime.TotalMilliseconds;
            base.Load();
        }

        public override void Unload()
        {
            base.Unload();
        }

        public override void ResetEffects()
        {
            showItemsCount = false;
        }

        public override bool HoverSlot(Item[] inventory, int context, int slot)
        {
            CheckDriveChestRemoveItem();
            return base.HoverSlot(inventory, context, slot);
        }

        public override bool OnPickup(Item item)
        {
            CheckDriveChestRemoveItem();

            return base.OnPickup(item);
        }

        public override void OnEnterWorld()
        {
            CheckDriveChestRemoveItem();
            base.OnEnterWorld();
        }

        private void CheckDriveChestRemoveItem()
        {
            if (Player.HasItem(ModContent.ItemType<Items.DriveChestRemoteItem>()))
                hasDriveRemoteItem = true;
            else
                hasDriveRemoteItem = false;
        }

        private void LongTick()
        {
            CheckDriveChestRemoveItem();
                
            if (hasDriveRemoteItem)
            {
                SatelliteStorage.driveChestSystem.DepositItemsFromInventory(true, true, true);
            }
        }

        public override void PostUpdate()
        {
            if (Main.gameTimeCache.TotalGameTime.TotalMilliseconds - longTickMilliseconds >= 330)
            {
                longTickMilliseconds = Main.gameTimeCache.TotalGameTime.TotalMilliseconds;
                LongTick();
            }

            base.PostUpdate();
        }


        public override void UpdateEquips()
        {
            foreach (Terraria.Item item in Player.armor)
            {
                if (item.type == ModContent.ItemType<Items.ItemsCountAccessoryItem>())
                {
                    showItemsCount = true;
                }
            }

            if (Player.HasItem(ModContent.ItemType<Items.ItemsCountAccessoryItem>()))
                showItemsCount = true;

        }

        public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
        {
            return
                SatelliteStorage.driveChestSystem.InventorySlotShift(inventory, context, slot)
                && 
                base.ShiftClickSlot(inventory, context, slot);
        }

        public override bool CanUseItem(Item item)
        {
            if (
                Main.netMode != NetmodeID.Server &&
                SatelliteStorage.driveChestUI.mouseOver
            ) return false;

            return base.CanUseItem(item);
        }

        public static bool CheckAdjChanged()
        {
            Player player = Main.LocalPlayer;

            List<bool> adjList = new List<bool>
            {
                player.adjHoney,
                player.adjLava,
                player.adjWater
            };

            foreach (bool b in player.adjTile)
            {
                adjList.Add(b);
            }

            if (oldAdjList == null || oldAdjList.Count != adjList.Count)
            {
                oldAdjList = adjList;
                return true;
            }

            for (int i = 0; i < adjList.Count; i++)
            {
                if (adjList[i] != oldAdjList[i])
                {
                    oldAdjList = adjList;
                    return true;
                }
            }

            return false;
        }

        public override void SaveData(TagCompound tag)
        {
            SatelliteStorage.Debug("Saving player data");

            IList<TagCompound> autoimportItemsCompound = new List<TagCompound>();

            foreach (var item in _autoImportItems)
            {
                autoimportItemsCompound.Add(DriveItemsSerializer.SerializeDriveItem(item.Value));
            }

            tag.Set("SatelliteStorage_AutoImportItems", autoimportItemsCompound);

            base.SaveData(tag);
        }

        public override void LoadData(TagCompound tag)
        {
            SatelliteStorage.Debug("Loading player data");

            IList<TagCompound> autoimportItems = 
                tag.GetList<TagCompound>("SatelliteStorage_AutoImportItems");

            ClearAutoImportItems();

            for (int i = 0; i < autoimportItems.Count; i++)
            {
                TagCompound itemCompound = autoimportItems[i];
                IDriveItem item = DriveItemsSerializer.DeserializeDriveItem(itemCompound);

                if (item == null) continue;

                if (!_autoImportItems.ContainsKey(item.type))
                    _autoImportItems.Add(item.type, item);

            }

            base.LoadData(tag);
        }
    }
}
