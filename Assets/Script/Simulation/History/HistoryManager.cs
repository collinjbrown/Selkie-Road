using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.Constructs;
using DeadReckoning.Map;
using DeadReckoning.WorldGeneration;
using System.Linq;

namespace DeadReckoning.Sim
{
    public class HistoryManager
    {
        public int Day { get { return day; } }
        private int day;

        public int worldPopulation = 0;
        public List<County> counties;
        public List<Civilization> civilizations;

        public void PassDay(WorldGeneration.HexSphereGenerator hGen)
        {
            int population = 0;
            day++;

            foreach (Civilization civ in civilizations)
            {
                civ.PassDay(hGen);

                population += civ.Population;
            }

            worldPopulation = population;
        }

        public void GenerateCounties(List<Tile> landTiles, int desiredCountySize)
        {
            List<Tile> unsortedTiles = landTiles;
            Dictionary<Tile, County> newCounties = new Dictionary<Tile, County>();
            counties = new List<County>();

            for (int i = 0; i < landTiles.Count / desiredCountySize; i++)
            {
                Tile randomTile = unsortedTiles[Random.Range(0, landTiles.Count)];
                newCounties.Add(randomTile, new County(randomTile));
                counties.Add(newCounties[randomTile]);
                unsortedTiles.Remove(randomTile);
            }

            foreach (Tile t in unsortedTiles)
            {
                if (t.hex.isWalkable)
                {
                    Tile closestTile = null;
                    float closestDistance = Mathf.Infinity;

                    foreach (KeyValuePair<Tile, County> tc in newCounties)
                    {
                        Tile c = tc.Key;

                        if ((t.hex.center.pos - c.hex.center.pos).sqrMagnitude < closestDistance)
                        {
                            closestDistance = (t.hex.center.pos - c.hex.center.pos).sqrMagnitude;
                            closestTile = c;
                        }
                    }

                    newCounties[closestTile].domain.Add(t);
                    t.county = newCounties[closestTile];
                }
            }
        }

        public void GenerateCivilizations(List<County> counties, int startingCivilizationCount)
        {
            civilizations = new List<Civilization>();
            List<County> unusedCounties = counties;

            for (int i = 0; i < startingCivilizationCount; i++)
            {
                County startingCounty = unusedCounties[Random.Range(0, unusedCounties.Count)];
                Civilization civ = new Civilization(startingCounty);
                civilizations.Add(civ);
                startingCounty.civ = civ;
            }
        }

        public HistoryManager(int startingCivilizationCount, int desiredCountySize, List<Tile> landTiles)
        {
            counties = new List<County>();
            civilizations = new List<Civilization>();

            day = 0;
            GenerateCounties(landTiles, desiredCountySize);
            GenerateCivilizations((from County c in counties where c.domain[0].Fertility > 10 select c).ToList(), startingCivilizationCount);
        }
    }
}
