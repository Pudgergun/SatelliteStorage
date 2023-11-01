using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatelliteStorage.ModNetwork
{
    public class PacketContext : IPacketContext
    {
        public int indexIncrement { get; set; } = 0;
        public bool hasPlayerIdReaded { get; set; } = false;
        public int messageType { get; private set; } = 0;
        public int whoAmI { get; private set; }
        public BinaryReader reader { get; private set; }
        public byte playerID { get; set; }
        public long readerPosition { get; set; }
        public bool needSeek { get; set; }
        public bool logsEnabled {  get; set; }

        public PacketContext(BinaryReader reader, int whoAmI)
        {
            this.reader = reader;
            messageType = reader.ReadByte();
            this.whoAmI = whoAmI;
        }
    }
}
