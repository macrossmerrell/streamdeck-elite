using System;
using System.Collections.Generic;

namespace Elite
{
    // Exact FSD jump-range maths, using the ship's real FSD stats from its Loadout event.
    //   fuel used = FuelMul * (distance * mass / OptimalMass) ^ FuelPower
    //   => range  = (OptimalMass / mass) * (fuel / FuelMul) ^ (1 / FuelPower)   (+ Guardian booster, flat)
    // Base module stats are from the EDCD/coriolis-data set; engineering changes to optimal mass /
    // max fuel per jump come straight from the Loadout modifiers. Verified against the game's own
    // MaxJumpRange (mass = unladen + max fuel per jump) to 5 decimal places.
    public static class FsdData
    {
        private static readonly Dictionary<string, (double OptMass, double MaxFuel, double FuelMul, double FuelPower)> Fsds =
            new Dictionary<string, (double, double, double, double)>(StringComparer.OrdinalIgnoreCase)
        {
            { "int_hyperdrive_overcharge_size6_class2", (1800, 8, 0.012, 2.6) },
            { "int_hyperdrive_overcharge_size6_class1", (1200, 5.3, 0.008, 2.6) },
            { "int_hyperdrive_overcharge_size6_class4", (1800, 8, 0.012, 2.6) },
            { "int_hyperdrive_overcharge_size6_class3", (1800, 8, 0.012, 2.6) },
            { "int_hyperdrive_overcharge_size5_class3", (1050, 5, 0.012, 2.45) },
            { "int_hyperdrive_overcharge_size5_class2", (1050, 5, 0.012, 2.45) },
            { "int_hyperdrive_overcharge_size5_class5", (1175, 5.2, 0.013, 2.45) },
            { "int_hyperdrive_overcharge_size5_class4", (1050, 5, 0.012, 2.45) },
            { "int_hyperdrive_overcharge_size6_class5", (2000, 8.3, 0.013, 2.6) },
            { "int_hyperdrive_overcharge_size8_class1", (2800, 13.6, 0.008, 2.90) },
            { "int_hyperdrive_overcharge_size7_class5", (3000, 13.1, 0.013, 2.75) },
            { "int_hyperdrive_overcharge_size8_class3", (4200, 20.4, 0.012, 2.90) },
            { "int_hyperdrive_overcharge_size8_class2", (4200, 20.4, 0.012, 2.90) },
            { "int_hyperdrive_overcharge_size7_class2", (2700, 12.8, 0.012, 2.75) },
            { "int_hyperdrive_overcharge_size7_class1", (1800, 8.5, 0.008, 2.75) },
            { "int_hyperdrive_overcharge_size7_class4", (2700, 12.8, 0.012, 2.75) },
            { "int_hyperdrive_overcharge_size7_class3", (2700, 12.8, 0.012, 2.75) },
            { "int_hyperdrive_overcharge_size2_class5", (100, 1, 0.013, 2.0) },
            { "int_hyperdrive_overcharge_size2_class4", (90, 0.9, 0.012, 2.0) },
            { "int_hyperdrive_overcharge_size3_class2", (150, 1.8, 0.012, 2.15) },
            { "int_hyperdrive_overcharge_size3_class1", (100, 1.2, 0.008, 2.15) },
            { "int_hyperdrive_overcharge_size2_class1", (60, 0.6, 0.008, 2.0) },
            { "int_missing_hyperdrive", (0, 0, 0, 0) },
            { "int_hyperdrive_overcharge_size2_class3", (90, 0.9, 0.012, 2.0) },
            { "int_hyperdrive_overcharge_size2_class2", (90, 0.9, 0.012, 2.0) },
            { "int_hyperdrive_overcharge_size3_class3", (150, 1.8, 0.012, 2.15) },
            { "int_hyperdrive_overcharge_size4_class4", (525, 3, 0.012, 2.3) },
            { "int_hyperdrive_overcharge_size4_class3", (525, 3, 0.012, 2.3) },
            { "int_hyperdrive_overcharge_size5_class1", (700, 3.3, 0.008, 2.45) },
            { "int_hyperdrive_overcharge_size4_class5", (585, 3.2, 0.013, 2.3) },
            { "int_hyperdrive_overcharge_size3_class5", (167, 1.9, 0.013, 2.15) },
            { "int_hyperdrive_overcharge_size3_class4", (150, 1.8, 0.012, 2.15) },
            { "int_hyperdrive_overcharge_size4_class2", (525, 3, 0.012, 2.3) },
            { "int_hyperdrive_overcharge_size4_class1", (350, 2, 0.008, 2.3) },
            { "int_hyperdrive_size5_class2", (630, 3.3, 0.01, 2.45) },
            { "int_hyperdrive_size5_class3", (700, 3.3, 0.008, 2.45) },
            { "int_hyperdrive_size6_class5", (1800, 8, 0.012, 2.6) },
            { "int_hyperdrive_size5_class1", (560, 3.3, 0.011, 2.45) },
            { "int_hyperdrive_size4_class1", (280, 2, 0.011, 2.3) },
            { "int_hyperdrive_size4_class2", (315, 2, 0.01, 2.3) },
            { "int_hyperdrive_size5_class4", (875, 4.1, 0.01, 2.45) },
            { "int_hyperdrive_size5_class5", (1050, 5, 0.012, 2.45) },
            { "int_hyperdrive_size6_class4", (1500, 6.6, 0.01, 2.6) },
            { "int_hyperdrive_size7_class3", (1800, 8.5, 0.008, 2.75) },
            { "int_hyperdrive_size7_class4", (2250, 10.6, 0.01, 2.75) },
            { "int_hyperdrive_size7_class1", (1440, 8.5, 0.011, 2.75) },
            { "int_hyperdrive_size7_class2", (1620, 8.5, 0.01, 2.75) },
            { "int_hyperdrive_size6_class2", (1080, 5.3, 0.01, 2.6) },
            { "int_hyperdrive_size6_class3", (1200, 5.3, 0.008, 2.6) },
            { "int_hyperdrive_size7_class5", (2700, 12.8, 0.012, 2.75) },
            { "int_hyperdrive_size6_class1", (960, 5.3, 0.011, 2.6) },
            { "int_hyperdrive_size4_class3", (350, 2, 0.008, 2.3) },
            { "int_hyperdrive_size2_class2", (54, 0.6, 0.01, 2) },
            { "int_hyperdrive_size2_class3", (60, 0.6, 0.008, 2) },
            { "int_hyperdrive_size3_class5", (150, 1.8, 0.012, 2.15) },
            { "int_hyperdrive_size2_class1", (48, 0.6, 0.011, 2) },
            { "int_hyperdrive_size2_class4", (75, 0.8, 0.01, 2) },
            { "int_hyperdrive_overcharge_size8_class5", (4670, 20.7, 0.013, 2.90) },
            { "int_hyperdrive_overcharge_size8_class4", (4200, 20.4, 0.012, 2.90) },
            { "int_hyperdrive_size2_class5", (90, 0.9, 0.012, 2) },
            { "int_hyperdrive_overcharge_size8_class5_overchargebooster_mkii", (4670, 6.8, 0.011, 2.5025) },
            { "int_hyperdrive_size3_class4", (125, 1.5, 0.01, 2.15) },
            { "int_hyperdrive_size3_class2", (90, 1.2, 0.01, 2.15) },
            { "int_hyperdrive_size4_class4", (437.5, 2.5, 0.01, 2.3) },
            { "int_hyperdrive_size4_class5", (525, 3, 0.012, 2.3) },
            { "int_hyperdrive_size3_class1", (80, 1.2, 0.011, 2.15) },
            { "int_hyperdrive_size3_class3", (100, 1.2, 0.008, 2.15) },
        };

        private static readonly Dictionary<string, double> Boosters = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            { "int_guardianfsdbooster_size1", 4.0 },
            { "int_guardianfsdbooster_size2", 6.0 },
            { "int_guardianfsdbooster_size3", 7.75 },
            { "int_guardianfsdbooster_size4", 9.25 },
            { "int_guardianfsdbooster_size5", 10.5 },
        };

        public static bool Known { get; private set; }
        public static double OptimalMass { get; private set; }
        public static double MaxFuelPerJump { get; private set; }
        public static double FuelMul { get; private set; }
        public static double FuelPower { get; private set; }
        public static double BoosterLy { get; private set; }

        // engineeredOptimalMass / engineeredMaxFuel: the Loadout modifier values (null/0 if not engineered).
        public static void Set(string fsdItem, double? engineeredOptimalMass, double? engineeredMaxFuel, string boosterItem)
        {
            Known = false;
            BoosterLy = 0;

            if (!string.IsNullOrEmpty(boosterItem) && Boosters.TryGetValue(boosterItem, out var boost))
                BoosterLy = boost;

            if (string.IsNullOrEmpty(fsdItem) || !Fsds.TryGetValue(fsdItem, out var s))
                return;

            OptimalMass = engineeredOptimalMass > 0 ? engineeredOptimalMass.Value : s.OptMass;
            MaxFuelPerJump = engineeredMaxFuel > 0 ? engineeredMaxFuel.Value : s.MaxFuel;
            FuelMul = s.FuelMul;
            FuelPower = s.FuelPower;
            Known = true;
        }

        // Jump range (Ly) for the given total ship mass and fuel currently available to the FSD.
        public static double Range(double totalMass, double fuelAvailable)
        {
            if (!Known || totalMass <= 0) return 0;

            var fuel = Math.Min(fuelAvailable, MaxFuelPerJump);
            if (fuel <= 0) return 0;

            return (OptimalMass / totalMass) * Math.Pow(fuel / FuelMul, 1.0 / FuelPower) + BoosterLy;
        }
    }
}
