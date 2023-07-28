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

namespace SatelliteStorage
{
    class SatelliteStoragePlayer : ModPlayer
    {

        private static List<bool> oldAdjList;

        public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
        {
            if (DriveChestUI.isDrawing)
            {
                if (Main.mouseItem.favorited) return false;

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
    }
}
