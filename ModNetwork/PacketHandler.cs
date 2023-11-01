using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace SatelliteStorage.ModNetwork
{
    class PacketHandler : IPacketHandler
    {
        private Dictionary<int, List<PacketHandlerStruct>> eventHandlers;

        public PacketHandler()
        {
            eventHandlers = new Dictionary<int, List<PacketHandlerStruct>>();
        }

        public void OnEvent<T>(int eventID, MethodInfo callback, object context) where T : IPacketWrapper
        {
            if (!eventHandlers.ContainsKey(eventID))
                eventHandlers.Add(eventID, new List<PacketHandlerStruct>());

            Type packetType = callback.GetParameters().First().ParameterType;

            eventHandlers[eventID].Add(new PacketHandlerStruct
            {
                callback = callback,
                packetType = packetType,
                context = context
            });
        }

        public void OffEvent(int eventID)
        {
            eventHandlers.Remove(eventID);
        }

        public void CallEvent(int eventID, object[] args)
        {
            if (!eventHandlers.ContainsKey(eventID)) return;

            foreach (PacketHandlerStruct handlerStruct in eventHandlers[eventID])
            {
                handlerStruct.callback.Invoke(handlerStruct.context, new object[]
                {
                    Activator.CreateInstance(handlerStruct.packetType, args)
                });
            }
        }

        public void OffEvents()
        {
            eventHandlers.Clear();
        }
    }
}
