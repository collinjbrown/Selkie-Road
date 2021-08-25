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
        public List<County> counties;

        public Civilization (HexSphereGenerator hGen, Tile startTile)
        {
            
        }
    }

    public class County
    {
        public Color color;
        public List<Structure> structures;
        public List<Tile> domain;

        public County(List<Tile> domain, Structure structure)
        {
            structures = new List<Structure>();
            structures.Add(structure);

            this.domain = domain;
            color = Random.ColorHSV();
        }
    }
}
