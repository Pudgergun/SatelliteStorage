using System.Collections.Generic;

namespace SatelliteStorage.Generators
{
    public interface IGenerator
    {
        int chance { get; }
        Generator AddDrop(int type, int count, int chance, int chanceType);
        GeneratorDropData GetDropData(int index);
        List<GeneratorDropData> GetDropList();
        int GetRandomDropIndex();
    }
}