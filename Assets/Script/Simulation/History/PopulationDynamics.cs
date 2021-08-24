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
        public const float growthRate = 0.0003f;
        public const int minorMigrationChance = 75;
        public const int majorMigrationChance = 95;

        #region Population
        public static float CalculateDailyGrowth(float currentPopulation, Tile tile)
        {
            float tileCapacity = tile.PopulationLimit;

            float calculatedGrowth = growthRate * currentPopulation * ((tileCapacity - currentPopulation) / tileCapacity);

            return calculatedGrowth / 100f;
        }

        public static float CalculateDailyDeaths()
        {
            // Chances for death by disease will increase with a larger population.
            // Or if there are animals present in large numbers.
            // Or if the climate is wet.

            // Death by exposure will increase due to low or high temperatures.

            // Death by famine would increase with lower fertility...
            // but I worry that would just decimate populations in low fertility zones,
            // so we'll probably have famine be a set rate for now.

            return 1f;
        }
        #endregion

        #region Migrations
        public static void CalculateMigration(HistoryManager manager, Structure structure)
        {
            if (Random.Range(0, 101) > minorMigrationChance)
            {
                float migrationSize = Random.Range(structure.floatingPopulation * 0.05f, structure.floatingPopulation * 0.1f);
                MinorMigration(manager, migrationSize, structure, true);
            }

            if ((structure.Population / structure.Tile.PopulationLimit) > 0.8f && Random.Range(0, 101) > majorMigrationChance)
            {
                // Migrate
                float migrationSize = Random.Range(structure.floatingPopulation * 0.1f, structure.floatingPopulation * 0.3f);
                MajorMigration(manager, migrationSize, structure, true);
            }
        }

        public static void MinorMigration(HistoryManager manager, float migrationSize, Structure origin, bool byLand)
        {
            if (byLand)
            {
                Structure bestNeighbor = null;
                float bestValue = Mathf.NegativeInfinity;

                foreach (KeyValuePair<Structure, List<Hex>> neighbors in origin.pathToNeighbors)
                {
                    if (neighbors.Value.Count > 0 && neighbors.Key.Tile.PopulationLimit - neighbors.Key.Population > bestValue)
                    {
                        bestNeighbor = neighbors.Key;
                        bestValue = neighbors.Key.Tile.PopulationLimit - neighbors.Key.Population;
                    }
                }

                if (bestNeighbor != null)
                {
                    Debug.Log($"A host of {Mathf.RoundToInt(migrationSize)} took a {origin.pathToNeighbors[bestNeighbor].Count * 40} mile trek to a new village ({origin.pathToNeighbors[bestNeighbor].Count} tiles).");
                    bestNeighbor.floatingPopulation += migrationSize;
                    origin.floatingPopulation -= migrationSize;
                }
            }
        }

        public static void MajorMigration(HistoryManager manager, float migrationSize, Structure origin, bool byLand)
        {
            // By land.
            if (byLand)
            {
                List<Hex> potentialDestinations = origin.Tile.hex.neighbors;

                Tile bestOption = null;
                float mostFertility = Mathf.NegativeInfinity;

                foreach (Hex h in potentialDestinations)
                {
                    if (h.tile.Fertility > mostFertility)
                    {
                        mostFertility = h.tile.Fertility;
                        bestOption = h.tile;
                    }
                }

                Structure s = new Structure(bestOption, migrationSize);
                origin.floatingPopulation -= migrationSize;
                manager.newStructures.Add(s);
            }
        }
        #endregion
    }
}
