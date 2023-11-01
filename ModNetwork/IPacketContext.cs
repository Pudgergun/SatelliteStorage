using System.IO;

namespace SatelliteStorage.ModNetwork
{
    public interface IPacketContext
    {
        int indexIncrement { get; set; }
        bool hasPlayerIdReaded { get; set; }
        int messageType { get; }
        int whoAmI { get; }
        BinaryReader reader { get; }
        long readerPosition { get; set; }
        bool needSeek { get; set; }
        byte playerID { get; set; }
        bool logsEnabled { get; set; }
    }
}