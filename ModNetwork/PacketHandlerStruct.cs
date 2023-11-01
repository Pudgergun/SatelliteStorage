using System;
using System.Reflection;

namespace SatelliteStorage.ModNetwork
{
    struct PacketHandlerStruct
    {
        public MethodInfo callback;
        public Type packetType;
        public object context;
    }
}
