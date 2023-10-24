using Terraria.ModLoader;
using Terraria;
using Terraria.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;
using Terraria.ID;
using log4net;
using SatelliteStorage.DriveSystem;
using SatelliteStorage.UI;
using Terraria.Audio;
using Terraria.GameInput;
using static Terraria.GameContent.Bestiary.IL_BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions;

namespace SatelliteStorage
{
    class SatelliteStoragePlayer : ModPlayer
    {

        private static List<bool> oldAdjList;
        private static double longTickMilliseconds;

        public bool showItemsCount { get; private set; }
        public bool hasDriveRemoteItem { get; private set; }

        

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

        public override void Load()
        {
            longTickMilliseconds = Main.gameTimeCache.TotalGameTime.TotalMilliseconds;
            base.Load();
        }

        public override void Unload()
        {
            base.Unload();
        }

        private void LongTick()
        {
            CheckDriveChestRemoveItem();
                
            if (hasDriveRemoteItem)
            {
                DriveChestSystemLocal.DepositItemsFromInventory(true, true, true);
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
            if (DriveChestUI.isDrawing)
            {
                if (Main.LocalPlayer.inventory[slot].IsAir) return false;
                if (Main.LocalPlayer.inventory[slot].favorited) return false;

                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    if (!DriveChestSystem.AddItem(DriveItem.FromItem(Main.LocalPlayer.inventory[slot]))) return false;
                    DriveChestUI.ReloadItems();
                    Main.LocalPlayer.inventory[slot].TurnToAir();
                    SoundEngine.PlaySound(SoundID.Grab);
                }

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    if (SatelliteStorage.AddDriveChestItemSended) return false;
                    SatelliteStorage.AddDriveChestItemSended = true;
                    ModPacket packet = SatelliteStorage.instance.GetPacket();
                    packet.Write((byte)SatelliteStorage.MessageType.AddDriveChestItem);
                    packet.Write((byte)Main.LocalPlayer.whoAmI);
                    packet.Write((byte)1);
                    packet.Write7BitEncodedInt(slot);

                    packet.Send();
                    packet.Close();
                }

                return false;
            }

            return base.ShiftClickSlot(inventory, context, slot);
        }

        public override bool CanUseItem(Item item)
        {
            if (UI.DriveChestUI.mouseOver) return false;
            return base.CanUseItem(item);
        }

        public static bool CheckAdjChanged()
        {
            Player player = Main.LocalPlayer;
            List<bool> adjList = new List<bool>();

            adjList.Add(player.adjHoney);
            adjList.Add(player.adjLava);
            adjList.Add(player.adjWater);

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
            IList<TagCompound> autoimportItemsCompound = new List<TagCompound>();

            foreach (var item in SatelliteStorage.AutoImportItems)
            {
                autoimportItemsCompound.Add(Utils.DriveItemsSerializer.SaveDriveItem(item.Value));
            }

            tag.Set("SatelliteStorage_AutoImportItems", autoimportItemsCompound);

            base.SaveData(tag);
        }

        public override void LoadData(TagCompound tag)
        {
            IList<TagCompound> autoimportItems = tag.GetList<TagCompound>("SatelliteStorage_AutoImportItems");

            SatelliteStorage.AutoImportItems.Clear();

            for (int i = 0; i < autoimportItems.Count; i++)
            {
                TagCompound itemCompound = autoimportItems[i];
                DriveItem item = Utils.DriveItemsSerializer.LoadDriveItem(itemCompound);

                if (!SatelliteStorage.AutoImportItems.ContainsKey(item.type))
                    SatelliteStorage.AutoImportItems.Add(item.type, item);

            }

            base.LoadData(tag);
        }
    }
}
