using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.Map;
using DeadReckoning.WorldGeneration;
using DeadReckoning.Constructs;

namespace DeadReckoning.Sim
{
    public class Host : MonoBehaviour
    {
        public Purpose purpose;
        public Civilization civ;
        public int number;

        int moveCount = 0;
        List<Hex> path;

        HistoryManager historyManager;

        public void Move()
        {
            moveCount++;

            // Debug.Log($"{moveCount} // {path.Count}");

            if (moveCount < path.Count)
            {
                this.transform.position = path[moveCount - 1].center.pos;
                this.transform.up = path[moveCount - 1].center.pos.normalized;
            }
            else
            {
                // You've arrived.
                if (purpose == Purpose.settlement)
                {
                    SettleTile();
                }
                else
                {
                    Debug.LogError("We shouldn't have military units yet.");
                }
            }
        }

        public void SettleTile()
        {
            Tile settledTile = path[path.Count - 1].tile;

            settledTile.county.civ = civ;

            Structure s = new Structure(settledTile.county, settledTile, number);
            settledTile.county.structures.Add(s);


            settledTile.hex.chunk.UpdateColors(Camera.main.GetComponent<GlobeCamera>().lens);

            historyManager.endHosts.Add(this);
            Destroy(this.gameObject, 0.1f);
        }

        public void Initiate(HistoryManager hManager, Civilization civ, int size, Purpose purpose, Hex start, Hex end)
        {
            historyManager = hManager;

            number = size;
            this.civ = civ;

            this.purpose = purpose;
            Pathfinding pathfinding = new Pathfinding(80000);
            path = pathfinding.FindPath(start, end);


        }
        public enum Purpose { settlement, military }
    }
}
