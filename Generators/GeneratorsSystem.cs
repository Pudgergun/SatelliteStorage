using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using SatelliteStorage.DriveSystem;
using SatelliteStorage.ModNetwork;

namespace SatelliteStorage.Generators
{
    public class GeneratorsSystem
    {
        private Dictionary<int, IGenerator> _generators = new Dictionary<int, IGenerator>();
        private Dictionary<int, int> _generatorsInv = new Dictionary<int, int>();
        private Dictionary<int, int> _generatedItemsQueue = new Dictionary<int, int>();
        private Mod _mod;

        public event Action<int, int> OnItemGenerated;

        public GeneratorsSystem(Mod mod)
        {
            _mod = mod;

            GeneratorsBootstrap.InitGenerators(this);
        }

        public Dictionary<int, IGenerator> GetGenerators() => _generators;

        public void AddGenerator(int type, IGenerator generator)
        {
            _generators.Add(type, generator);
        }

        public void ClearInv()
        {
            _generatorsInv.Clear();
        }

        public void InitGeneratorsInv(Dictionary<int, int> generators)
        {
            this._generatorsInv = generators;
        }

        public Dictionary<int, int> GetGeneratorsInv()
        {
            return _generatorsInv;
        }

        public void SetGeneratorsInvSlot(int type, int count)
        {
            _generatorsInv[type] = count;
        }

        public void AddGeneratorToInv(byte type)
        {
            if (_generatorsInv.ContainsKey(type)) _generatorsInv[type]++;
            else _generatorsInv[type] = 1;
            SyncGenerator(type);
        }

        public void TakeGeneratorFromInv(byte type)
        {
            if (!_generatorsInv.ContainsKey(type)) return;
            _generatorsInv[type]--;
            if (_generatorsInv[type] < 0) _generatorsInv[type] = 0;
            SyncGenerator(type);
        }

        public void SyncGenerator(byte type, int to = -1)
        {
            if (Main.netMode != NetmodeID.Server) return;

            var packet = _mod.GetPacket();
            packet.Write((byte)MessageType.SyncGeneratorState);
            packet.Write((byte)type);
            packet.Write7BitEncodedInt(_generatorsInv[type]);
            packet.Send(to);
            packet.Close();
        }

        public void SyncAllGeneratorsTo(int to)
        {
            foreach (byte key in _generatorsInv.Keys)
            {
                SyncGenerator(key, to);
            }
        }

        public void OnGeneratorsTick()
        {
            if (Main.netMode != NetmodeID.SinglePlayer && Main.netMode != NetmodeID.Server) return;

            Random random = new Random();

            foreach (int key in _generatorsInv.Keys)
            {
                for (int i = 0; i < _generatorsInv[key]; i++)
                {
                    IGenerator generator = _generators[key];

                    if (random.Next(0, 100) <= generator.chance)
                    {
                        int index = generator.GetRandomDropIndex();
                        GeneratorDropData data = generator.GetDropData(index);

                        if (_generatedItemsQueue.ContainsKey(data.type)) _generatedItemsQueue[data.type] += data.count;
                        else _generatedItemsQueue.Add(data.type, data.count);
                    }
                }
            }

            foreach (int key in _generatedItemsQueue.Keys)
            {
                OnItemGenerated.Invoke(key, _generatedItemsQueue[key]);
            }

            _generatedItemsQueue.Clear();
        }
    }
}
