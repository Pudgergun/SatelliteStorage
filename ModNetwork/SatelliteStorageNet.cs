using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.UI;

namespace SatelliteStorage.ModNetwork
{
    class SatelliteStorageNet : ISatelliteStorageNet
    {
        private IPacketHandler _packetHandler;
        private Mod _mod;
        private Dictionary<Type, MethodInfo> _onEventGenerics;

        public SatelliteStorageNet(Mod mod)
        {
            _packetHandler = new PacketHandler();
            _onEventGenerics = new Dictionary<Type, MethodInfo>();

            SearchAndCacheMethodsWithAttribute();

            _mod = mod;
        }

        private void SearchAndCacheMethodsWithAttribute()
        {
            MethodInfo onEventMethod = GetType().GetTypeInfo().GetMethod("OnEvent", BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var method in GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var attribute = method.GetCustomAttribute<NetEvent>();

                if (attribute == null) continue;

                Type packetType = method.GetParameters().First().ParameterType;

                MethodInfo onEventGeneric;

                if (!_onEventGenerics.ContainsKey(packetType))
                    _onEventGenerics.Add(packetType, onEventMethod.MakeGenericMethod(packetType));

                onEventGeneric = _onEventGenerics[packetType];

                onEventGeneric.Invoke(this, new object[] { attribute.messageType, method });
            }
        }

        public ModPacket GetModPacket()
        {
            return _mod.GetPacket();
        }

        public void HandlePacket(IPacketContext context)
        {
            _packetHandler.CallEvent(context.messageType, new object[] { context });
        }

        protected void OnEvent<T>(MessageType messageType, MethodInfo method) where T : IPacketWrapper
        {
            _packetHandler.OnEvent<T>((int)messageType, method, this);
        }
    }
}
