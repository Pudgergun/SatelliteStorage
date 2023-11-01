using MonoMod.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace SatelliteStorage.ModNetwork
{
    class PacketWrapper : IPacketWrapper
    {
        public int messageType { get; private set; }
        public BinaryReader reader { get; private set; }
        public int whoAmI { get; private set; }

        private int _index;

      
        public PacketWrapper(IPacketContext context)
        {
            messageType = context.messageType;
            whoAmI = context.whoAmI;
            reader = context.reader;
            _index = context.indexIncrement;

            if (context.needSeek)
                reader.BaseStream.Seek(context.readerPosition, SeekOrigin.Begin);
            else
                context.readerPosition = reader.BaseStream.Position;

            context.needSeek = true;

            if (context.logsEnabled)
                SatelliteStorage.Debug(
                    $"\n=========== HandlePacket '{Enum.GetName(typeof(MessageType), messageType)}' [{_index}]" +
                    $"\n=========== StreamLength: {reader.BaseStream.Length}, ReaderPosition: {context.readerPosition}\n"
                );

            context.indexIncrement++;
        }
    }
}
