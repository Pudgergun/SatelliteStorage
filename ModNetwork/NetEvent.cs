using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatelliteStorage.ModNetwork
{
    [AttributeUsage(AttributeTargets.Method)]
    class NetEvent : Attribute
    {
        public MessageType messageType { get; private set; }

        public NetEvent(MessageType messageType)
        {
            this.messageType = messageType;
        }
    }
}
