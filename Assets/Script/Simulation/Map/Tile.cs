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

        public void Generate(HexSphereGenerator hGen)
        {
            foreach (Tile t in tiles)
            {
                t.DetermineWinds(hGen);
                t.DetermineSubmerged(hGen);
            }
            foreach (Tile t in tiles)
            {
                t.DetermineCurrents(hGen);
            }
        }

        public TileMap (List<Tile> t)
        {
            tiles = t;
        }
    }

    public class Tile
    {
        public bool submerged;

        public List<Resource> resourceYields;
        
        public List<Structure> structures;

        public Biome biome;

        public Vector3 prevailingWind;
        public WindType windType;

        public Tile currentDirection;
        public CurrentHeat currentHeat;

        public Hex hex;

        public Tile (Hex h)
        {
            hex = h;
        }

        public enum Biome { forest, desert, mountain, plain }
        public enum WindType { trade, westerly, easterly }
        public enum CurrentHeat { lukewarm, warm, cold }

        #region Worldbuilding
        public void DetermineWinds(HexSphereGenerator hGen)
        {
            float worldRadius = hGen.worldRadius;
            Vector3 worldCenter = hGen.transform.position;

            Vector3 pos = this.hex.center.pos;
            Vector3[] relativeAxes = HexChunk.FindRelativeAxes(this.hex.center);

            if (Mathf.Abs(pos.y - worldCenter.y) <= worldRadius * 0.35f) // Really 30
            {
                // Trade Winds
                this.hex.windColor = Color.blue;
                this.prevailingWind = -relativeAxes[2];
                windType = WindType.trade;
            }
            else if (Mathf.Abs(pos.y - worldCenter.y) <= worldRadius * 0.75f) // Really 60
            {
                // Westerlies

                this.hex.windColor = Color.red;
                this.prevailingWind = relativeAxes[2];
                windType = WindType.westerly;
            }
            else
            {
                // Polar Easterlies

                this.hex.windColor = Color.cyan;
                this.prevailingWind = -relativeAxes[2];
                windType = WindType.easterly;
            }
        }

        public void DetermineSubmerged(HexSphereGenerator hGen)
        {
            if (hex.center.pos.magnitude < hGen.oceanRadius)
            {
                submerged = true;
            }
        }

        public void DetermineCurrents(HexSphereGenerator hGen)
        {
            float equatorY = hGen.transform.position.y;

            if (!submerged && !hex.pent)
            {
                if (windType == WindType.trade || windType == WindType.easterly)
                {
                    // Warm on right, cold on left.
                    // This is overly simplistic, but we'll flesh this out later.

                    Hex leftNeighbor = hex.neighbors[2];
                    Hex rightNeighbor = hex.neighbors[5];

                    if (leftNeighbor.tile.submerged)
                    {
                        leftNeighbor.tile.currentHeat = CurrentHeat.cold; // This tile is on the left side of land.

                        if (hex.center.pos.y >= equatorY)
                        {
                            if (!CheckNeighborSubmerge(leftNeighbor, 3))
                            {
                                CheckNeighborSubmerge(leftNeighbor, 4);
                            }
                        }
                        else if (hex.center.pos.y < equatorY)
                        {
                            if (!CheckNeighborSubmerge(leftNeighbor, 0))
                            {
                                CheckNeighborSubmerge(leftNeighbor, 1);
                            }
                        }
                    }

                    if (rightNeighbor.tile.submerged)  // This tile is on the right side of land.
                    {
                        rightNeighbor.tile.currentHeat = CurrentHeat.warm;

                        if (hex.center.pos.y < equatorY)
                        {
                            if (!CheckNeighborSubmerge(rightNeighbor, 3))
                            {
                                CheckNeighborSubmerge(rightNeighbor, 4);
                            }
                        }
                        else if (hex.center.pos.y >= equatorY)
                        {
                            if (!CheckNeighborSubmerge(rightNeighbor, 0))
                            {
                                CheckNeighborSubmerge(rightNeighbor, 1);
                            }
                        }
                    }
                }
                else if (windType == WindType.westerly)
                {

                    Hex leftNeighbor = hex.neighbors[2];
                    Hex rightNeighbor = hex.neighbors[5];

                    if (leftNeighbor.tile.submerged)
                    {
                        leftNeighbor.tile.currentHeat = CurrentHeat.cold; // This tile is on the left side of land.

                        if (hex.center.pos.y < equatorY)
                        {
                            if (!CheckNeighborSubmerge(leftNeighbor, 3))
                            {
                                CheckNeighborSubmerge(leftNeighbor, 4);
                            }
                        }
                        else if (hex.center.pos.y >= equatorY)
                        {
                            if (!CheckNeighborSubmerge(leftNeighbor, 0))
                            {
                                CheckNeighborSubmerge(leftNeighbor, 1);
                            }
                        }
                    }

                    if (rightNeighbor.tile.submerged)  // This tile is on the right side of land.
                    {
                        rightNeighbor.tile.currentHeat = CurrentHeat.warm;

                        if (hex.center.pos.y >= equatorY)
                        {
                            if (!CheckNeighborSubmerge(rightNeighbor, 3))
                            {
                                CheckNeighborSubmerge(rightNeighbor, 4);
                            }
                        }
                        else if (hex.center.pos.y < equatorY)
                        {
                            if (!CheckNeighborSubmerge(rightNeighbor, 0))
                            {
                                CheckNeighborSubmerge(rightNeighbor, 1);
                            }
                        }
                    }

                }
            }


            if (currentHeat == CurrentHeat.warm)
            {
                Debug.Log("Tile is warm.");
                hex.currentColor = Color.red;
            }
            else if (currentHeat == CurrentHeat.cold)
            {
                Debug.Log("Tile is cold.");
                hex.currentColor = Color.cyan;
            }
            else
            {
                hex.currentColor = Color.white;
            }
        }

        bool CheckNeighborSubmerge(Hex h, int neighboy) // That's not a typo.
        {
            // Come up with a way to better map the currents on the coast.

            if (h.neighbors[neighboy].tile.submerged)
            {
                h.tile.currentDirection = h.neighbors[neighboy].tile;
                return true;
            }

            return false;
        }
        #endregion
    }

    public class Resource
    {
        public int yield;
        public Type type;

        public enum Type { stone, wood, food, metal } // These are just placeholders.
    }
}
