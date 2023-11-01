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

namespace SatelliteStorage.Utils
{
    public class DriveItemsSerializer
    {
        public static TagCompound SerializeDriveItem(IDriveItem item)
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

        public static IDriveItem DeserializeDriveItem(TagCompound tag, int version = 1)
        {
            IDriveItem item = new DriveItem();

            string name = tag.GetString("name");

            if (name == "default" || version <= 0)
            {
                item.SetType(tag.GetInt("type"));
            } else
            {
                Mod itemMod;
                if (!ModLoader.TryGetMod(tag.GetString("mod"), out itemMod)) return null;
                ModItem outitem;
                bool itemFound = itemMod.TryFind(name, out outitem);
                if (!itemFound) return null;
                item.SetType(outitem.Type);
            }
            
            item.SetStack(tag.GetInt("stack"));
            int prefix = tag.GetInt("prefix");

            if (prefix != 0) item.SetPrefix(prefix);
            return item;
        }

        public static ModPacket WriteDriveItemsToPacket(List<IDriveItem> items, ModPacket packet)
        {
            packet.Write7BitEncodedInt(items.Count);

            for (int i = 0; i < items.Count; i++)
            {
                IDriveItem item = items[i];
                packet.Write7BitEncodedInt(item.type);
                packet.Write7BitEncodedInt(item.stack);
                packet.Write7BitEncodedInt(item.prefix);
            }

            return packet;
        }

        public static List<IDriveItem> ReadDriveItems(BinaryReader reader)
        {
            List<IDriveItem> items = new List<IDriveItem>();

            int count = reader.Read7BitEncodedInt();

            for (int i = 0; i < count; i++)
            {
                items.Add(new DriveItem()
                    .SetType(reader.Read7BitEncodedInt())
                    .SetStack(reader.Read7BitEncodedInt())
                    .SetPrefix(reader.Read7BitEncodedInt()));
            }

            return items;
        }
    }
}
