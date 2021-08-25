using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.Map;
using DeadReckoning.Sim;
using DeadReckoning.WorldGeneration;

namespace DeadReckoning.Constructs
{
    public class Structure
    {
        public int Population { get { return Mathf.RoundToInt(floatingPopulation); } }

        public float floatingPopulation;

        public void GrowPopulation() { floatingPopulation += floatingPopulation * PopulationDynamics.CalculateDailyGrowth(floatingPopulation, Tile); }
        public void CullPopulation() { floatingPopulation -= floatingPopulation * PopulationDynamics.CalculateDailyDeaths(this); }

        public int buildings;
        public int NecessaryBuildings { get { return 1 + (Population / 100); } }

        public Tile Tile { get; }
        public Dictionary<Resource, int> stores;

        public Dictionary<Structure, List<Hex>> pathToNeighbors;

        Government Government { get; }
        Culture Culture { get; }
        Religion Religion { get; }
        Language Language { get; }

        #region Resources
        public void ReapAndConsume()
        {
            foreach (KeyValuePair<Resource.Type, Resource> r in Tile.resources)
            {
                if (stores.ContainsKey(r.Value))
                {
                    // Reap
                    stores[r.Value] += Mathf.RoundToInt(r.Value.yield * (Population / Tile.PopulationLimit));

                    // Consume
                    stores[r.Value] -= Mathf.RoundToInt((Population / (Tile.tileArea * 2.5f)) / Tile.Fertility);

                    // Decay
                    stores[r.Value] -= 1;

                    if (stores[r.Value] < 0)
                    {
                        stores[r.Value] = 0;
                    }
                }
                else
                {
                    // Reap
                    stores.Add(r.Value, Mathf.RoundToInt(r.Value.yield * (Population / Tile.PopulationLimit)));

                    // Consume
                    stores[r.Value] -= Mathf.RoundToInt((Population / (Tile.tileArea * 2.5f)) / Tile.Fertility);

                    // Decay
                    stores[r.Value] -= 1;

                    if (stores[r.Value] < 0)
                    {
                        stores[r.Value] = 0;
                    }
                }
            }
        }
        #endregion

        #region Constructors
        public Structure(Tile tile, float startingPop)
        {
            pathToNeighbors = new Dictionary<Structure, List<Hex>>();
            stores = new Dictionary<Resource, int>();

            tile.structures.Add(this);
            floatingPopulation = startingPop;
            Tile = tile;
            Government = new Government((Government.Type)Random.Range(0, 3));
            Culture = new Culture(true, true);
            Religion = new Religion();
            Language = new Language(true, true, "of");
        }

        public Structure(Tile tile, Government government, Culture culture, Religion religion, Language language)
        {
            pathToNeighbors = new Dictionary<Structure, List<Hex>>();
            stores = new Dictionary<Resource, int>();

            tile.structures.Add(this);
            Tile = tile;
            Government = government;
            Culture = culture;
            Religion = religion;
            Language = language;
        }
        #endregion
    }

    public class Government
    {
        public Type type;

        public enum Type { feudal, tribal, nomadic }

        #region Constructors
        public Government(Type type)
        {
            this.type = type;
        }
        #endregion
    }

    public class Culture
    {
        public bool SurnamingScheme { get; }
        public bool OriginNamingScheme { get; }

        #region Constructors
        public Culture (bool surname, bool origin)
        {
            SurnamingScheme = surname;
            OriginNamingScheme = origin;
        }
        #endregion
    }

    public class Religion
    {

    }

    public class Language
    {
        public bool ForwardNamingScheme { get; }
        public bool HeadInitial { get; }
        public string BasicGenitive { get; }

        #region Constructors
        public Language (bool forward, bool initial, string genitive)
        {
            ForwardNamingScheme = forward;
            HeadInitial = initial;
            BasicGenitive = genitive;
        }
        #endregion
    }
}
