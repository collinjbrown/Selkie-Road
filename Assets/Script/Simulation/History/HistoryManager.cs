using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.Constructs;
using DeadReckoning.Map;
using DeadReckoning.WorldGeneration;

namespace DeadReckoning.Sim
{
    public class HistoryManager
    {
        public int Day { get { return day; } }
        private int day;

        public int worldPopulation;

        public int StructureCount { get { return structures.Count; } }
        public List<Structure> structures;
        public List<Structure> newStructures;

        public void PassDay(WorldGeneration.HexSphereGenerator hGen)
        {
            foreach (Structure s in newStructures)
            {
                foreach (Structure st in structures)
                {
                    Pathfinding pathfinder = new Pathfinding(10000);
                    List<Hex> path = pathfinder.FindPath(st.Tile.hex, s.Tile.hex);

                    st.pathToNeighbors.Add(s, path);

                    List<Hex> revPath = path;

                    if (path.Count > 0)
                    {
                        revPath.Reverse();
                    }

                    s.pathToNeighbors.Add(st, revPath);
                }

                structures.Add(s);
            }

            newStructures = new List<Structure>();

            day++;
            int copulation = 0; // Sorry.

            foreach (Structure s in structures)
            {
                // Update floating population.
                int oldPopulation = s.Population;
                s.GrowPopulation();
                s.CullPopulation();

                copulation += s.Population;

                s.ReapAndConsume();
                // Debug.Log($"{oldPopulation} --> {s.Population}");

                // PopulationDynamics.CalculateMigration(this, s);

                if (s.buildings < s.NecessaryBuildings)
                {
                    Vector3 randLoc = Vector3.Lerp(s.Tile.hex.vertices[Random.Range(0, s.Tile.hex.vertices.Length)].pos, s.Tile.hex.vertices[Random.Range(0, s.Tile.hex.vertices.Length)].pos, Random.Range(0, 1f));
                    randLoc = Vector3.Lerp(randLoc, s.Tile.hex.center.pos, Random.Range(0.1f, 0.9f));
                    s.Tile.hex.chunk.AddBuilding(new Procedural.ProceduralGeneration.Building(hGen.procSettings, Procedural.ProceduralGeneration.Building.Type.hut, randLoc));
                    s.buildings++;
                }
            }

            worldPopulation = copulation;
        }

        public HistoryManager(int startingCount, List<Tile> landTiles)
        {
            day = 0;
            structures = new List<Structure>();
            newStructures = new List<Structure>();
            int copulation = 0;

            while (structures.Count < startingCount)
            {
                Tile randomTile = landTiles[Random.Range(0, landTiles.Count)];
                landTiles.Remove(randomTile);

                if (randomTile.PopulationLimit > 0)
                {
                    // Create a new structure.
                    float startingPop = Random.Range(20f, 1000f);
                    Structure structure = new Structure(randomTile, startingPop);
                    structures.Add(structure);
                    copulation += structure.Population;
                }
            }

            worldPopulation = copulation;
        }
    }
}
