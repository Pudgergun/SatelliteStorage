using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SatelliteStorage.SatelliteStorage;
using Terraria.ID;

namespace SatelliteStorage.Generators
{
    class GeneratorsBootstrap
    {
        public static void InitGenerators(GeneratorsSystem generatorsSystem)
        {
            generatorsSystem.AddGenerator((int)GeneratorTypes.BaseGenerator,
                new Generator(25)
                .AddDrop(ItemID.TinBar, 1, 100, (int)GeneratorChanceType.VeryHighChance)
                .AddDrop(ItemID.CopperBar, 1, 100, (int)GeneratorChanceType.VeryHighChance)
                .AddDrop(ItemID.IronBar, 1, 50, (int)GeneratorChanceType.HighChance)
                .AddDrop(ItemID.LeadBar, 1, 50, (int)GeneratorChanceType.HighChance)
                .AddDrop(ItemID.GoldBar, 1, 25, (int)GeneratorChanceType.AverageChance)
                .AddDrop(ItemID.PlatinumBar, 1, 25, (int)GeneratorChanceType.AverageChance)
                .AddDrop(ItemID.Diamond, 1, 5, (int)GeneratorChanceType.VeryLowChance)
                .AddDrop(ItemID.Amber, 1, 5, (int)GeneratorChanceType.VeryLowChance)
                .AddDrop(ItemID.Ruby, 1, 5, (int)GeneratorChanceType.VeryLowChance)
                .AddDrop(ItemID.Emerald, 1, 5, (int)GeneratorChanceType.VeryLowChance)
                .AddDrop(ItemID.Sapphire, 1, 5, (int)GeneratorChanceType.VeryLowChance)
                .AddDrop(ItemID.Topaz, 1, 5, (int)GeneratorChanceType.VeryLowChance)
                .AddDrop(ItemID.Amethyst, 1, 5, (int)GeneratorChanceType.VeryLowChance)
            );

            generatorsSystem.AddGenerator((int)GeneratorTypes.HellstoneGenerator,
                new Generator(15)
                .AddDrop(ItemID.HellstoneBar, 1, 25, (int)GeneratorChanceType.AverageChance)
            );

            generatorsSystem.AddGenerator((int)GeneratorTypes.MeteoriteGenerator,
                new Generator(15)
                .AddDrop(ItemID.MeteoriteBar, 1, 100, (int)GeneratorChanceType.AverageChance)
            );

            generatorsSystem.AddGenerator((int)GeneratorTypes.ShroomiteGenerator,
                new Generator(25)
                .AddDrop(ItemID.GlowingMushroom, 1, 100, (int)GeneratorChanceType.VeryHighChance)
                .AddDrop(ItemID.ChlorophyteBar, 1, 15, (int)GeneratorChanceType.VeryLowChance)
            );

            generatorsSystem.AddGenerator((int)GeneratorTypes.SpectreGenerator,
                new Generator(25)
                .AddDrop(ItemID.Bone, 1, 100, (int)GeneratorChanceType.VeryHighChance)
                .AddDrop(ItemID.Ectoplasm, 1, 50, (int)GeneratorChanceType.HighChance)
                .AddDrop(ItemID.ChlorophyteBar, 1, 15, (int)GeneratorChanceType.VeryLowChance)
            );

            generatorsSystem.AddGenerator((int)GeneratorTypes.LuminiteGenerator,
                new Generator(15)
                .AddDrop(ItemID.LunarBar, 1, 50, (int)GeneratorChanceType.HighChance)
                .AddDrop(ItemID.FragmentSolar, 1, 50, (int)GeneratorChanceType.HighChance)
                .AddDrop(ItemID.FragmentNebula, 1, 50, (int)GeneratorChanceType.HighChance)
                .AddDrop(ItemID.FragmentVortex, 1, 50, (int)GeneratorChanceType.HighChance)
                .AddDrop(ItemID.FragmentStardust, 1, 50, (int)GeneratorChanceType.HighChance)
            );

            generatorsSystem.AddGenerator((int)GeneratorTypes.ChlorophyteGenerator,
                new Generator(25)
                .AddDrop(ItemID.JungleSpores, 1, 100, (int)GeneratorChanceType.VeryHighChance)
                .AddDrop(ItemID.Stinger, 1, 100, (int)GeneratorChanceType.VeryHighChance)
                .AddDrop(ItemID.Vine, 1, 100, (int)GeneratorChanceType.VeryHighChance)
                .AddDrop(ItemID.ChlorophyteBar, 1, 50, (int)GeneratorChanceType.HighChance)
            );

            generatorsSystem.AddGenerator((int)GeneratorTypes.HallowedGenerator,
                new Generator(25)
                .AddDrop(ItemID.SilverCoin, 1, 100, (int)GeneratorChanceType.VeryHighChance)
                .AddDrop(ItemID.HallowedBar, 1, 25, (int)GeneratorChanceType.LowChance)
                .AddDrop(ItemID.SuperHealingPotion, 1, 5, (int)GeneratorChanceType.VeryLowChance)
                .AddDrop(ItemID.SuperManaPotion, 1, 5, (int)GeneratorChanceType.VeryLowChance)
            );

            generatorsSystem.AddGenerator((int)GeneratorTypes.SoulGenerator,
                new Generator(25)
                .AddDrop(ItemID.SoulofFlight, 1, 100, (int)GeneratorChanceType.VeryHighChance)
                .AddDrop(ItemID.SoulofLight, 1, 100, (int)GeneratorChanceType.VeryHighChance)
                .AddDrop(ItemID.SoulofNight, 1, 100, (int)GeneratorChanceType.VeryHighChance)
            );

            generatorsSystem.AddGenerator((int)GeneratorTypes.PowerGenerator,
                new Generator(25)
                .AddDrop(ItemID.SoulofMight, 1, 100, (int)GeneratorChanceType.VeryHighChance)
                .AddDrop(ItemID.SoulofSight, 1, 100, (int)GeneratorChanceType.VeryHighChance)
                .AddDrop(ItemID.SoulofFright, 1, 100, (int)GeneratorChanceType.VeryHighChance)
            );
        }
    }
}
