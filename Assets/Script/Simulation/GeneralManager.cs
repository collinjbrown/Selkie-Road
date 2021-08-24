using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.Map;

namespace DeadReckoning.Sim
{
    public class GeneralManager : MonoBehaviour
    {
        public int totalPopulation;
        public static int startingStructures = 10;

        public HistoryManager HistoryManager;
        public WorldGeneration.HexSphereGenerator hGen;

        public void PassDay()
        {
            // We'll add more here once we have more stuff.
            HistoryManager.PassDay(hGen);
            totalPopulation = HistoryManager.worldPopulation;
        }

        public void PassDays(int days)
        {
            for (int i = 0; i < days; i++)
            {
                HistoryManager.PassDay(hGen);
            }
            totalPopulation = HistoryManager.worldPopulation;
        }

        public void Setup(List<Tile> landTiles)
        {
            HistoryManager = new HistoryManager(startingStructures, landTiles);
            totalPopulation = HistoryManager.worldPopulation;
        }
    }
}
