using SatelliteStorage.DriveSystem;
using SatelliteStorage.Generators;
using SatelliteStorage.ModNetwork;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace SatelliteStorage.GeneratorsNetwork
{
    class GeneratorsServer : SatelliteStorageNet
    {
        private GeneratorsSystem _generatorsSystem;

        public GeneratorsServer(Mod mod, GeneratorsSystem generatorsSystem) : base(mod)
        {
            _generatorsSystem = generatorsSystem;
        }

        [NetEvent(MessageType.RequestStates)]
        private void OnRequestStates(EventPacketWithPlayer data)
        {
            _generatorsSystem.SyncAllGeneratorsTo(data.playerID);
        }

        [NetEvent(MessageType.ChangeGeneratorState)]
        private void OnChangeGeneratorState(EventPacketWithPlayer data)
        {
            byte generatorType = data.reader.ReadByte();
            byte changeTo = data.reader.ReadByte();

            if (changeTo == 1)
                _generatorsSystem.AddGeneratorToInv(generatorType);
            else
                _generatorsSystem.AddGeneratorToInv(generatorType);
        }
    }
}
