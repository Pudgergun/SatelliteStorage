﻿using Terraria.ModLoader;
using Terraria;
using Terraria.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;
using Terraria.ID;
using log4net;
using SatelliteStorage.DriveSystem;

namespace SatelliteStorage.Utils
{
    public class DriveItemsSerializer
    {
        public static TagCompound SaveDriveItem(DriveItem item)
        {
            TagCompound tag = new TagCompound();
            tag["type"] = item.type;

            ModItem modItem = item.ToItem().ModItem;

            if (modItem != null)
            {
                tag["mod"] = modItem.Mod.Name;
                tag["name"] = modItem.Name;
            } else
            {
                tag["name"] = "default";
            }
            
            tag["stack"] = item.stack;
            tag["prefix"] = item.prefix;
            return tag;
        }

        public static DriveItem LoadDriveItem(TagCompound tag, int version = 1)
        {
            DriveItem item = new DriveItem();

            string name = tag.GetString("name");

            if (name == "default" || version <= 0)
            {
                item.type = tag.GetInt("type");
            } else
            {
                try
                {
                    Mod itemMod = ModLoader.GetMod(tag.GetString("mod"));
                    if (itemMod == null) return null;
                    ModItem outitem;
                    bool itemFound = itemMod.TryFind(name, out outitem);
                    if (!itemFound) return null;
                    item.type = outitem.Type;

                } catch (KeyNotFoundException exc)
                {
                    return null;
                }
            }
            
            item.stack = tag.GetInt("stack");
            int prefix = tag.GetInt("prefix");

            if (prefix != 0) item.prefix = prefix;
            return item;
        }

        public static ModPacket WriteDriveItemsToPacket(List<DriveItem> items, ModPacket packet)
        {
            packet.Write7BitEncodedInt(items.Count);

            for (int i = 0; i < items.Count; i++)
            {
                DriveItem item = items[i];
                packet.Write7BitEncodedInt(item.type);
                packet.Write7BitEncodedInt(item.stack);
                packet.Write7BitEncodedInt(item.prefix);
            }

            return packet;
        }

        public static List<DriveItem> ReadDriveItems(BinaryReader reader)
        {
            List<DriveItem> items = new List<DriveItem>();

            int count = reader.Read7BitEncodedInt();

            for (int i = 0; i < count; i++)
            {
                DriveItem item = new DriveItem();
                item.type = reader.Read7BitEncodedInt();
                item.stack = reader.Read7BitEncodedInt();
                item.prefix = reader.Read7BitEncodedInt();
                items.Add(item);
            }

            return items;
        }
    }
}
