using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatelliteStorage.ModNetwork
{
    class EventPacketWithPlayer : PacketWrapper
    {
        public byte playerID { get; private set; }

        public EventPacketWithPlayer(IPacketContext context) : base(context)
        {
            context.playerID = reader.ReadByte();
            context.hasPlayerIdReaded = true;

            playerID = context.playerID;
        }
    }
}
