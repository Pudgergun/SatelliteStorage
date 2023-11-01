using System.Collections.Generic;
using SatelliteStorage.Utils;

namespace SatelliteStorage.Generators
{
    public class Generator : IGenerator
    {
        public int chance { get; private set; } = 0;

        private List<int> dropsChances = new List<int>();
        private List<GeneratorDropData> drops = new List<GeneratorDropData>();

        public Generator(int chance)
        {
            this.chance = chance;
        }

        public Generator AddDrop(int type, int count, int chance, int chanceType)
        {
            drops.Add(new GeneratorDropData
            {
                type = type,
                count = count,
                chance = chance,
                chanceType = chanceType
            });

            dropsChances.Add(chance);
            return this;
        }

        public int GetRandomDropIndex()
        {
            int index = RandomUtils.Roulette(dropsChances);
            return index;
        }

        public GeneratorDropData GetDropData(int index)
        {
            return drops[index];
        }

        public List<GeneratorDropData> GetDropList() => drops;
    }
}