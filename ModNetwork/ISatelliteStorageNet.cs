using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace SatelliteStorage.ModNetwork
{
    interface ISatelliteStorageNet
    {
        public ModPacket GetModPacket();
        public void HandlePacket(IPacketContext data);
    }
}
