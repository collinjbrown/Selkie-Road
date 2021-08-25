using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.Constructs;
using DeadReckoning.Map;
using DeadReckoning.WorldGeneration;

namespace DeadReckoning.Sim
{
    public class Civilization
    {
        public int Population { get { return GetPopulation(); } }

        public Color color;
        public List<County> counties;

        public void PassDay(HexSphereGenerator hGen)
        {
            foreach (County c in counties)
            {
                c.PassDay(hGen);
            }
        }

        int GetPopulation()
        {
            int retVal = 0;

            foreach (County c in counties)
            {
                retVal += c.Population;
            }

            return retVal;
        }

        public Civilization (County startingCounty)
        {
            counties = new List<County>();
            counties.Add(startingCounty);
            startingCounty.civ = this;

            color = Random.ColorHSV();
        }
    }

    public class County
    {
        public int Population { get { return GetPopulation(); } }

        public Color color;
        public List<Structure> structures;
        public List<Tile> domain;
        public Civilization civ;

        public void PassDay(HexSphereGenerator hGen)
        {
            foreach (Structure s in structures)
            {
                s.PassDay(hGen);
            }
        }

        int GetPopulation()
        {
            int retVal = 0;

            foreach (Structure s in structures)
            {
                retVal += s.Population;
            }

            return retVal;
        }

        public County(Tile startingTile)
        {
            structures = new List<Structure>();
            domain = new List<Tile>();
            domain.Add(startingTile);
            startingTile.county = this;

            color = Random.ColorHSV();

            Structure s = new Structure(this, startingTile, Random.Range(100, 1001));
            structures.Add(s);
        }
    }
}
