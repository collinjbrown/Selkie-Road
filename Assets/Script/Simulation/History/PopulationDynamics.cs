using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.Map;
using DeadReckoning.Constructs;
using DeadReckoning.WorldGeneration;

namespace DeadReckoning.Sim
{
    public static class PopulationDynamics
    {
        public const float growthRate = 0.000027f;
        public const int minorMigrationChance = 75;
        public const int majorMigrationChance = 99;

        #region Population
        public static float CalculateDailyGrowth(float currentPopulation, Tile tile)
        {
            float tileCapacity = tile.PopulationLimit;

            float calculatedGrowth = growthRate * currentPopulation * ((tileCapacity - currentPopulation) / tileCapacity);

            // We're going to limit this to true growth rather, as famine, disease, and war should keep population in check.
            if (calculatedGrowth < 0)
            {
                calculatedGrowth = 0;
            }

            // Debug.Log($"Growth: {calculatedGrowth}");
            return calculatedGrowth / 100;
        }

        public static float CalculateDailyDeaths(Structure structure)
        {
            // Chances for death by disease will increase with a larger population.
            // Or if there are animals present in large numbers.
            // Or if the climate is wet.

            // Death by exposure will increase due to low or high temperatures.

            // Death by famine would increase with lower fertility...
            // but I worry that would just decimate populations in low fertility zones,
            // so we'll probably have famine be a set rate for now.

            float deathPercentage = 0;

            #region Disease
            float populationModifier = structure.Population / 1000f;
            float livestockModifier = structure.Tile.GetLivestockCount() / 20f;
            float wetnessModifier = 0;

            if (structure.Tile.precipitation == Tile.Gradient.high)
            {
                wetnessModifier += 0.5f;
            }
            else if (structure.Tile.precipitation == Tile.Gradient.veryHigh)
            {
                wetnessModifier += 1f;
            }

            float diseaseModifier = populationModifier + livestockModifier + wetnessModifier;
            deathPercentage += Random.Range(0, diseaseModifier);
            #endregion

            #region Exposure
            float temperatureModifier = 0;

            if (structure.Tile.temperature == Tile.Gradient.high || structure.Tile.temperature == Tile.Gradient.low)
            {
                temperatureModifier += 0.5f;
            }
            else if (structure.Tile.temperature == Tile.Gradient.veryHigh || structure.Tile.temperature == Tile.Gradient.veryLow)
            {
                temperatureModifier += 1f;
            }

            deathPercentage += Random.Range(0, temperatureModifier);
            #endregion

            // Debug.Log($"Death: {deathPercentage / 100}");
            return deathPercentage / 10000f;
        }
        #endregion

        #region Migrations
        public static void CalculateMigration(Structure structure)
        {
            //if (Random.Range(0,101) > minorMigrationChance)
            //{

            //}

            if ((structure.Population / structure.Tile.PopulationLimit) > 0.25f && Random.Range(0, 101) > majorMigrationChance)
            {
                Hex bestTarget = null;
                float bestValue = Mathf.NegativeInfinity;

                foreach (Tile t in structure.County.domain)
                {
                    foreach(Hex h in t.hex.neighbors)
                    {
                        if (h.tile.county != null)
                        {
                            if (h.tile.county.civ == null && h.tile.Fertility > bestValue)
                            {
                                bestTarget = h;
                                bestValue = h.tile.Fertility;
                            }
                        }
                    }
                }

                if (bestTarget != null)
                {
                    GeneralManager.instance.GenerateHost(structure.County.civ, Mathf.RoundToInt(structure.Population * Random.Range(0.1f, 0.3f)), Host.Purpose.settlement, structure.Tile.hex, bestTarget);
                }
            }
        }
        #endregion
    }
}
