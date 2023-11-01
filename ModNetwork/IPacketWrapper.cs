


using System.IO;

namespace SatelliteStorage.ModNetwork
{
    internal interface IPacketWrapper
    {
        int messageType { get; }
        BinaryReader reader { get; }
        int whoAmI { get; }
    }
}