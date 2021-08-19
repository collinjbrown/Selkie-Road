using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.WorldGeneration;
using DeadReckoning.Abstracts;


namespace DeadReckoning.Map
{
    public class TileMap
    {
        public List<Tile> tiles;

        public TileMap (List<Tile> t)
        {
            tiles = t;
        }
    }

    public class Tile
    {
        public List<Resource> resourceYields;
        public List<Structure> structures;

        public Hex hex;

        public Tile (Hex h)
        {
            hex = h;
        }
    }

    public class Resource
    {
        public int yield;
        public Type type;

        public enum Type { stone, wood, food, metal } // These are just placeholders.
    }
}
