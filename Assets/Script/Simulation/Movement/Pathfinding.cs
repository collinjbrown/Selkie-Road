using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.WorldGeneration;
using System.Linq;

namespace DeadReckoning.Map {
    public class Pathfinding
    {
        int searchDepth { get; }

        public Pathfinding(int sD)
        {
            searchDepth = sD;
        }

        public List<Hex> FindPath(Hex origin, Hex target)
        {
            List<Hex> openSet = new List<Hex>();
            List<Hex> closedSet = new List<Hex>();

            openSet.Add(origin);
            origin.gCost = 0;

            int depthSearched = 0;

            while (openSet.Count > 0)
            {
                if (depthSearched > searchDepth)
                {
                    Debug.Log($"No path found between {origin} & {target}.");
                    return new List<Hex>();
                }
                else
                {
                    depthSearched++;
                }

                Hex currentHex = openSet[0];

                if (currentHex == target)
                {
                    // Debug.Log($"Path found between {origin} & {target}.");
                    ClearCosts(openSet, closedSet);
                    return ReconstructPath(origin, target);
                }

                openSet.Remove(currentHex);
                closedSet.Add(currentHex);

                foreach (Hex n in currentHex.neighbors)
                {
                    if (!closedSet.Contains(n) && n.isWalkable)
                    {
                        float tentativeGCost = currentHex.gCost + (currentHex.center.pos - n.center.pos).sqrMagnitude + n.movementPenalty;

                        if (tentativeGCost < n.gCost)
                        {
                            n.parentHex = currentHex;
                            n.gCost = tentativeGCost;
                            n.hCost = (n.center.pos - target.center.pos).sqrMagnitude;

                            if (!openSet.Contains(n))
                            {
                                openSet.Add(n);
                                openSet.OrderBy(p => p.FCost).ToList();
                            }
                        }
                    }
                }
            }

            Debug.Log("Ho boy.");
            return new List<Hex>();
        }

        void ClearCosts(List<Hex> open, List<Hex> closed)
        {
            foreach (Hex h in open)
            {
                h.gCost = Mathf.Infinity;
                h.hCost = 0;
            }
            foreach (Hex h in closed)
            {
                h.gCost = Mathf.Infinity;
                h.hCost = 0;
            }
        }

        List<Hex> ReconstructPath(Hex start, Hex end)
        {
            List<Hex> path = new List<Hex>();
            Hex current = end;

            while (current != start)
            {
                path.Add(current.parentHex);
                current = current.parentHex;
            }

            path.Reverse();

            return path;
        }
    }
}
