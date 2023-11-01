using SatelliteStorage.DriveSystem;
using SatelliteStorage.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using SatelliteStorage.ModNetwork;
using System.Diagnostics;
using System.Collections;

namespace SatelliteStorage.DriveChestNetwork
{
    partial class DriveChestServer : SatelliteStorageNet
    {
        private DriveChestSystem _driveChestSystem;

        public DriveChestServer(Mod mod, DriveChestSystem driveChestSystem) : base(mod) {
            _driveChestSystem = driveChestSystem;
        }

        [NetEvent(MessageType.RequestDriveChestItems)]
        private void OnRequestDriveChestItems(EventPacketWithPlayer data)
        {
            ModPacket packet = GetModPacket();
            packet.Write((byte)MessageType.ResponseDriveChestItems);
            packet.Write(data.playerID);
            packet.Write(data.reader.ReadByte());
            DriveItemsSerializer.WriteDriveItemsToPacket(_driveChestSystem.GetItems(), packet);
            packet.Send(data.playerID);
            packet.Close();
        }

        [NetEvent(MessageType.SetSputnikState)]
        private void OnSetSputnikState(EventPacketWithPlayer data)
        {
            _driveChestSystem.isSputnikPlaced = data.reader.ReadByte() == 1;
            _driveChestSystem.SyncIsSputnikPlacedToClients();
        }

        [NetEvent(MessageType.RequestSputnikState)]
        private void OnRequestSputnikState(EventPacketWithPlayer data)
        {
            _driveChestSystem.SendSputnikState(data.playerID);
            _driveChestSystem.SendSyncItemsCount(data.playerID);
        }

        [NetEvent(MessageType.RequestStates)]
        private void OnRequestStates(EventPacketWithPlayer data)
        {
            _driveChestSystem.SendSputnikState(data.playerID);
            _driveChestSystem.SendSyncItemsCount(data.playerID);
        }

        [NetEvent(MessageType.TakeDriveChestItem)]
        private void OnTakeDriveChestItem(EventPacketWithPlayer data)
        {
            
            void SendTakeDriveVoidPacket()
            {
                var takeItemVoidPacket = GetModPacket();
                takeItemVoidPacket.Write((byte)MessageType.TakeDriveChestItem);
                takeItemVoidPacket.Write(data.playerID);
                takeItemVoidPacket.Write(false);
                takeItemVoidPacket.Write7BitEncodedInt(0);
                takeItemVoidPacket.Write7BitEncodedInt(0);
                takeItemVoidPacket.Write7BitEncodedInt(0);
                takeItemVoidPacket.Write((byte)0);
                takeItemVoidPacket.Write7BitEncodedInt(0);
                takeItemVoidPacket.Send(data.playerID);
                takeItemVoidPacket.Close();
            }

            ClickType clickType = (ClickType)data.reader.ReadByte();

            Item tmouseItem = Main.player[data.playerID].inventory[58];
            Player tdplayer = Main.player[data.playerID];
            SatelliteStoragePlayer modPlayer = tdplayer.GetModPlayer<SatelliteStoragePlayer>();
            bool itemTaked = false;

            int takeItemType = data.reader.Read7BitEncodedInt();
            int takeItemPrefix = data.reader.Read7BitEncodedInt();

            IDriveItem hoverDriveItem = new DriveItem()
                .SetType(takeItemType)
                .SetPrefix(takeItemPrefix);

            Item hoverItem = new Item();
            hoverItem.type = takeItemType;
            hoverItem.SetDefaults(hoverItem.type);
            hoverItem.stack = _driveChestSystem.GetItemCount(hoverDriveItem);

            int slotToAdd = -1;
            int countToAdd = 0;

            Item takeItem;

            int type = 0;
            int stack = 0;
            int prefix = 0;

            bool isTMouseItemAir = tmouseItem.IsAir;
            bool isTMouseItemSame = tmouseItem.type == takeItemType;

            if (!isTMouseItemAir && !isTMouseItemSame)
            {
                SendTakeDriveVoidPacket();
                return;
            }

            if (clickType == ClickType.RightClick)
            {
                if (!isTMouseItemAir && !isTMouseItemSame)
                {
                    SendTakeDriveVoidPacket();
                    return;
                }

                if (isTMouseItemSame)
                {
                    if (tmouseItem.stack + 1 > tmouseItem.maxStack)
                    {
                        SendTakeDriveVoidPacket();
                        return;
                    }
                }
            }
            if (clickType == ClickType.ShiftLeftClick)
            {
                if (!isTMouseItemAir)
                {
                    SendTakeDriveVoidPacket();
                    return;
                }

                int stackDiff = 0;

                for (int s = 0; s < 50; s++)
                {
                    if (tdplayer.inventory[s].type != ItemID.None &&
                        tdplayer.inventory[s].type == hoverItem.type &&
                        tdplayer.inventory[s].stack < hoverItem.maxStack &&
                        hoverItem.maxStack - tdplayer.inventory[s].stack > stackDiff &&
                        !tdplayer.inventory[s].favorited
                    )
                    {
                        stackDiff = hoverItem.maxStack - tdplayer.inventory[s].stack;
                        countToAdd = stackDiff;
                        if (countToAdd > hoverItem.stack) countToAdd = hoverItem.stack;
                        slotToAdd = s;
                    }
                }

                if (slotToAdd <= -1)
                {
                    for (int s = 0; s < 50; s++)
                    {
                        if (slotToAdd <= -1 && tdplayer.inventory[s].type == ItemID.None)
                        {
                            slotToAdd = s;
                            countToAdd = hoverItem.maxStack;
                            if (countToAdd > hoverItem.stack) countToAdd = hoverItem.stack;
                        }
                    }
                }
            }
            else
            {
                if (!isTMouseItemAir && clickType == ClickType.LeftClick)
                {
                    SendTakeDriveVoidPacket();
                    return;
                }
            }

            if (clickType == ClickType.ShiftLeftClick && slotToAdd <= -1)
            {
                SendTakeDriveVoidPacket();
                return;
            }

            int takeCount = clickType == ClickType.RightClick ? 1 : 0;
            if (clickType == ClickType.ShiftLeftClick) takeCount = countToAdd;

            if (
                !isTMouseItemAir &&
                modPlayer.TempTakeItemType != tmouseItem.type
            )
            {
                modPlayer.TempTakeItemType = tmouseItem.type;
                modPlayer.TempTakeItemCount = tmouseItem.stack;
            }

            if (
                !isTMouseItemAir &&
                modPlayer.TempTakeItemType == tmouseItem.type &&
                modPlayer.TempTakeItemCount > 0 &&
                modPlayer.TempTakeItemCount + takeCount > tmouseItem.maxStack
            )
            {
                SendTakeDriveVoidPacket();
                return;
            }
            
            takeItem = _driveChestSystem.TakeItem(takeItemType, takeItemPrefix, takeCount);
            if (takeItem != null)
            {
                if (clickType == ClickType.RightClick)
                {
                    if (isTMouseItemAir)
                    {
                        itemTaked = true;
                        type = takeItem.type;
                        stack = takeItem.stack;
                        prefix = takeItem.prefix;
                        modPlayer.TempTakeItemCount = stack;
                        modPlayer.TempTakeItemType = type;
                    }
                    else
                    {
                        itemTaked = true;
                        type = takeItem.type;
                        stack = modPlayer.TempTakeItemCount += takeItem.stack;
                        prefix = takeItem.prefix;
                    }
                }
                else if (clickType == ClickType.ShiftLeftClick)
                {
                    if (tdplayer.inventory[slotToAdd].type == ItemID.None)
                    {
                        tdplayer.inventory[slotToAdd] = takeItem.Clone();
                    }
                    else
                    {
                        tdplayer.inventory[slotToAdd].stack += countToAdd;
                    }

                    type = tdplayer.inventory[slotToAdd].type;
                    stack = tdplayer.inventory[slotToAdd].stack;
                    prefix = tdplayer.inventory[slotToAdd].prefix;
                    itemTaked = true;
                }
                else
                {
                    itemTaked = true;
                    type = takeItem.type;
                    stack = takeItem.stack;
                    prefix = takeItem.prefix;
                }
            }


            var takeItemPacket = GetModPacket();
            takeItemPacket.Write((byte)MessageType.TakeDriveChestItem);
            takeItemPacket.Write(data.playerID);
            takeItemPacket.Write(itemTaked);
            takeItemPacket.Write7BitEncodedInt(type);
            takeItemPacket.Write7BitEncodedInt(stack);
            takeItemPacket.Write7BitEncodedInt(prefix);
            takeItemPacket.Write((byte)clickType);
            takeItemPacket.Write7BitEncodedInt(slotToAdd);
            takeItemPacket.Send(data.playerID);
            takeItemPacket.Close();
        }

        [NetEvent(MessageType.AddDriveChestItem)]
        private void OnAddDriveChestItem(EventPacketWithPlayer data)
        {
            int addDriveChestItemType = data.reader.ReadByte();
            int fromSlot = 58;

            if (addDriveChestItemType == 1)
                fromSlot = data.reader.Read7BitEncodedInt();

            Item amouseItem = Main.player[data.playerID].inventory[fromSlot];

            bool added = false;

            IDriveItem addItem = new DriveItem()
                .SetType(amouseItem.type)
                .SetStack(amouseItem.stack)
                .SetPrefix(amouseItem.prefix);

            if (!amouseItem.IsAir && _driveChestSystem.AddItem(addItem))
            {
                amouseItem.TurnToAir();
                added = true;
            }

            var addItemPacket = GetModPacket();
            addItemPacket.Write((byte)MessageType.AddDriveChestItem);
            addItemPacket.Write(data.playerID);
            addItemPacket.Write(added);
            addItemPacket.Write7BitEncodedInt(fromSlot);
            addItemPacket.Send(data.playerID);
            addItemPacket.Close();
        }

        [NetEvent(MessageType.TryCraftRecipe)]
        private void OnTryCraftRecipe(EventPacketWithPlayer data)
        {
            int recipeID = data.reader.Read7BitEncodedInt();
            Recipe recipe = Main.recipe[recipeID];
            Player pl = Main.player[data.playerID];
            Item[] plInvItems = pl.inventory;
            Item plMouseItem = plInvItems[58];
            SatelliteStoragePlayer modPlayer = pl.GetModPlayer<SatelliteStoragePlayer>();

            bool isMouseItemAir = plMouseItem.IsAir;
            bool isMouseItemSame = plMouseItem.type == recipe.createItem.type;

            if (!isMouseItemAir && !isMouseItemSame) return; 

            if (isMouseItemSame)
            {
                if (plMouseItem.stack + recipe.createItem.stack > plMouseItem.maxStack) return;
            }

            if (
                !isMouseItemAir &&
                modPlayer.TempCraftItemType != plMouseItem.type
            )
            {
                modPlayer.TempCraftItemType = plMouseItem.type;
                modPlayer.TempCraftItemCount = plMouseItem.stack;
            }

            if (
                !isMouseItemAir &&
                modPlayer.TempCraftItemType == plMouseItem.type &&
                modPlayer.TempCraftItemCount > 0 &&
                modPlayer.TempCraftItemCount + recipe.createItem.stack > plMouseItem.maxStack
            )
            {
                return;
            }

            List<RecipeItemStruct> uses = _driveChestSystem.GetItemsUsesForCraft(plInvItems, recipe);
            if (uses == null) return;

            Dictionary<int, int> changedInvSlots = new Dictionary<int, int>();

            uses.ForEach(u =>
            {
                Item item = new Item();
                item.type = u.type;
                item.SetDefaults(item.type);
                item.stack = 1;

                if (u.from == 0)
                {
                    plInvItems[u.slot].stack -= u.stack;
                    if (plInvItems[u.slot].stack <= 0) plInvItems[u.slot].TurnToAir();

                    changedInvSlots[u.slot] = plInvItems[u.slot].stack;
                }
                else
                {
                    _driveChestSystem.SubItem(u.type, u.stack);
                }
            });

            if (isMouseItemAir)
            {
                plMouseItem = recipe.createItem.Clone();
                modPlayer.TempCraftItemCount = plMouseItem.stack;
                modPlayer.TempCraftItemType = plMouseItem.type;
            }
            else
            {
                modPlayer.TempCraftItemCount += recipe.createItem.stack;
                plMouseItem.stack += recipe.createItem.stack;
            }

            var tryCraftItemPacket = GetModPacket();
            tryCraftItemPacket.Write((byte)MessageType.TryCraftRecipe);
            tryCraftItemPacket.Write(data.playerID);
            tryCraftItemPacket.Write7BitEncodedInt(plMouseItem.type);
            tryCraftItemPacket.Write7BitEncodedInt(modPlayer.TempCraftItemCount);
            tryCraftItemPacket.Write7BitEncodedInt(plMouseItem.prefix);
            tryCraftItemPacket.Write7BitEncodedInt(changedInvSlots.Keys.Count);
            foreach (int key in changedInvSlots.Keys)
            {
                tryCraftItemPacket.Write7BitEncodedInt(key);
                tryCraftItemPacket.Write7BitEncodedInt(changedInvSlots[key]);
            }
            tryCraftItemPacket.Send(data.playerID);
            tryCraftItemPacket.Close();
        }

        [NetEvent(MessageType.DepositDriveChestItem)]
        private void OnDepositDriveChestItem(EventPacketWithPlayer data)
        {
            int depositType = data.reader.ReadByte();
            byte invSlot = data.reader.ReadByte();

            Item invItem = Main.player[data.playerID].inventory[invSlot];

            if (invItem.IsAir) return;

            IDriveItem depositItem = new DriveItem()
                .SetType(invItem.type)
                .SetStack(invItem.stack)
                .SetPrefix(invItem.prefix);


            if ((depositType == 1 ? true : _driveChestSystem.HasItem(depositItem)) && _driveChestSystem.AddItem(depositItem))
            {
                invItem.TurnToAir();

                var depositItemPacket = GetModPacket();
                depositItemPacket.Write((byte)MessageType.DepositDriveChestItem);
                depositItemPacket.Write(data.playerID);
                depositItemPacket.Write(invSlot);
                depositItemPacket.Send(data.playerID);
                depositItemPacket.Close();
            }
        }
    }
}
