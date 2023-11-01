using System.Reflection;

namespace SatelliteStorage.ModNetwork
{
    internal interface IPacketHandler
    {
        void CallEvent(int eventID, object[] args);
        void OffEvent(int eventID);
        void OffEvents();
        void OnEvent<T>(int eventID, MethodInfo callback, object context) where T : IPacketWrapper;
    }
}