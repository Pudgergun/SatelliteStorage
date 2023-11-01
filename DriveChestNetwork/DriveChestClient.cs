using SatelliteStorage.DriveSystem;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using SatelliteStorage.ModNetwork;
using SatelliteStorage.Utils;
using SatelliteStorage.UI;

namespace SatelliteStorage.DriveChestNetwork
{
    class DriveChestClient : SatelliteStorageNet
    {
        private DriveChestSystem _driveChestSystem;
        private UIDriveChest _driveChestUI;

        public DriveChestClient(Mod mod, DriveChestSystem driveChestSystem, UIDriveChest driveChestUI) : base(mod) {
            (_driveChestSystem, _driveChestUI) = (driveChestSystem, driveChestUI);
        }

        [NetEvent(MessageType.ResponseDriveChestItems)]
        private void OnResponseDriveChestItems(EventPacketWithPlayer data)
        {
            bool checkPosition = data.reader.ReadByte() == 1;
            List<IDriveItem> items = DriveItemsSerializer.ReadDriveItems(data.reader);
            _driveChestSystem.InitItems(items);
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Main.playerInventory = true;

            _driveChestUI.SetState(true);

            _driveChestSystem.CallDriveChestOpened(checkPosition, Main.LocalPlayer.position);
        }

        [NetEvent(MessageType.TakeDriveChestItem)]
        private void OnTakeDriveChestItem(EventPacketWithPlayer data)
        {
            bool itemTaked = data.reader.ReadBoolean();
            Item takeItem = new Item();
            takeItem.type = data.reader.Read7BitEncodedInt();
            takeItem.SetDefaults(takeItem.type);
            takeItem.stack = data.reader.Read7BitEncodedInt();
            takeItem.prefix = data.reader.Read7BitEncodedInt();

            byte clickType = data.reader.ReadByte();
            int invslot = data.reader.Read7BitEncodedInt();

            if (itemTaked)
            {
                if (clickType == 2)
                {
                    SoundEngine.PlaySound(SoundID.Grab);
                    _driveChestSystem.takeDriveChestItemSended = false;
                    Main.LocalPlayer.inventory[invslot] = takeItem;
                    return;
                }

                if (Main.mouseItem.IsAir)
                    Main.mouseItem = takeItem;
                else
                    Main.mouseItem.stack = takeItem.stack;
                
                    
               
                if (clickType == 1)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.Grab);
                }
            }

            _driveChestSystem.takeDriveChestItemSended = false;
        }

        [NetEvent(MessageType.AddDriveChestItem)]
        private void OnAddDriveChestItem(EventPacketWithPlayer data)
        {
            bool added = data.reader.ReadBoolean();
            int fromslot = data.reader.Read7BitEncodedInt();
            Item mouseItem = Main.LocalPlayer.inventory[fromslot];

            if (added)
            {
                mouseItem.TurnToAir();
                Main.mouseItem.TurnToAir();
                SoundEngine.PlaySound(SoundID.Grab);
            }

            _driveChestSystem.addDriveChestItemSended = false;
        }

        [NetEvent(MessageType.DepositDriveChestItem)]
        private void OnDepositDriveChestItem(EventPacketWithPlayer data)
        {
            int invSlot = data.reader.ReadByte();

            Item item = Main.LocalPlayer.inventory[invSlot];
            item.TurnToAir();
        }


        [NetEvent(MessageType.SetSputnikState)]
        private void OnSetSputnikState(EventPacket data)
        {
            int state = data.reader.ReadByte();

            _driveChestSystem.isSputnikPlaced = state == 1;
        }

        [NetEvent(MessageType.TryCraftRecipe)]
        private void OnTryCraftRecipe(EventPacketWithPlayer data)
        {
            int mItemType = data.reader.Read7BitEncodedInt();
            int mItemStack = data.reader.Read7BitEncodedInt();
            int mItemPrefix = data.reader.Read7BitEncodedInt();

            Item mItem = Main.LocalPlayer.inventory[58];

            mItem.type = mItemType;
            mItem.SetDefaults(mItem.type);

            mItem.stack = mItemStack;
            mItem.prefix = mItemPrefix;

            Main.mouseItem = mItem;

            int subInvItemsCount = data.reader.Read7BitEncodedInt();

            for (int i = 0; i < subInvItemsCount; i++)
            {
                int slot = data.reader.Read7BitEncodedInt();
                int count = data.reader.Read7BitEncodedInt();
                Main.LocalPlayer.inventory[slot].stack = count;
                if (Main.LocalPlayer.inventory[slot].stack <= 0) Main.LocalPlayer.inventory[slot].TurnToAir();
            }

            SoundEngine.PlaySound(SoundID.Grab);
            _driveChestSystem.checkRecipesRefresh = false;
        }

        [NetEvent(MessageType.SyncDriveChestItem)]
        private void OnSyncDriveChestItem(EventPacket data)
        {

            _driveChestSystem.SyncItem(
                new DriveItem()
                .SetType(data.reader.Read7BitEncodedInt())
                .SetStack(data.reader.Read7BitEncodedInt())
                .SetPrefix(data.reader.Read7BitEncodedInt())
                );

            _driveChestUI.ReloadItems();
        }

        [NetEvent(MessageType.StorageItemsCount)]
        private void OnStorageItemsCount(EventPacket data)
        {
            _driveChestSystem.SetItemsCount(data.reader.Read7BitEncodedInt());
        }
    }
}
