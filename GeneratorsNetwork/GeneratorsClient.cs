using SatelliteStorage.DriveSystem;
using SatelliteStorage.Generators;
using SatelliteStorage.ModNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace SatelliteStorage.GeneratorsNetwork
{
    class GeneratorsClient : SatelliteStorageNet
    {
        private GeneratorsSystem _generatorsSystem;

        public GeneratorsClient(Mod mod, GeneratorsSystem generatorsSystem) : base(mod)
        {
            _generatorsSystem = generatorsSystem;
        }

        [NetEvent(MessageType.SyncGeneratorState)]
        private void OnSyncGeneratorState(EventPacket data)
        {
            byte generatorType = data.reader.ReadByte();
            int generatorCount = data.reader.Read7BitEncodedInt();

            _generatorsSystem.SetGeneratorsInvSlot(generatorType, generatorCount);
        }
    }
}
