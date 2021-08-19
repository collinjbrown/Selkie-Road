using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.WorldGeneration;

namespace DeadReckoning.Map {
    public class WindSim
    {
        float equaterY;
        float worldRadius;

        public List<WindNode> nodes;
        public Dictionary<Vertex, WindNode> windtionary;

        public void UpdateWinds()
        {
            foreach (WindNode n in nodes)
            {
                n.AverageNeighbors();
                Debug.DrawRay(n.center.pos, n.normal * n.mag, n.windColor, 0.5f);
            }
        }

        public void RandomizeWinds()
        {
            foreach (WindNode n in nodes)
            {
                n.mag = Random.Range(0.1f, 1f);
                n.normal = (HexChunk.FindRelativeAxes(n.center)[1] + HexChunk.FindRelativeAxes(n.center)[2]) / Random.Range(1f, 2f);
                // Debug.DrawRay(n.center.pos, n.normal * n.mag, Color.red, 10f);
            }
        }

        public class WindNode
        {
            WindSim controller;

            public Vector3 naturalFlow;
            public float baseMag;

            public Vector3 normal;
            public float mag;

            public Vertex center;
            public List<Vertex> neighbors;

            public Color windColor;

            public WindType windType;

            public enum WindType { trade, westerly, easterly }

            public void AverageNeighbors()
            {
                Vector3 unaveragedNormal = naturalFlow + normal;
                float unaveragedMag = baseMag + mag;

                foreach (Vertex v in neighbors)
                {
                    WindNode w = controller.windtionary[v];

                    unaveragedNormal += w.normal;
                    unaveragedMag += w.mag;
                }

                normal = unaveragedNormal / (neighbors.Count + 1f); // Might be int.
                mag = unaveragedMag / (neighbors.Count + 1f);
            }

            public void SetWind()
            {
                float worldRadius = controller.worldRadius;
                float worldY = controller.equaterY;

                Vector3 pos = center.pos;
                Vector3[] relativeAxes = HexChunk.FindRelativeAxes(center);

                if (Mathf.Abs(pos.y - worldY) <= worldRadius * 0.35f) // Really 30
                {
                    // Trade Winds

                    windColor = Color.blue;
                    naturalFlow = -relativeAxes[2];
                    baseMag = 1 + (worldRadius * 0.35f / Mathf.Abs(pos.y - worldY));
                    windType = WindType.trade;
                }
                else if (Mathf.Abs(pos.y - worldY) <= worldRadius * 0.75f) // Really 60
                {
                    // Westerlies

                    windColor = Color.red;
                    naturalFlow = relativeAxes[2];
                    baseMag = 1 + (worldRadius * 0.75f / Mathf.Abs(pos.y - worldY));
                    windType = WindType.westerly;
                }
                else
                {
                    // Polar Easterlies

                    windColor = Color.cyan;
                    naturalFlow = -relativeAxes[2];
                    baseMag = 1 + (worldRadius / Mathf.Abs(pos.y - worldY));
                    windType = WindType.easterly;
                }

                normal = naturalFlow;
                mag = baseMag;
            }

            public WindNode(WindSim sim, Vertex v, List<Vertex> neighboys) // That is not a typo.
            {
                controller = sim;
                sim.nodes.Add(this);
                sim.windtionary.Add(v, this);
                center = v;
                neighbors = neighboys;

                SetWind();
            }
        }

        public WindSim(HexSphereGenerator hGen)
        {
            nodes = new List<WindNode>();
            windtionary = new Dictionary<Vertex, WindNode>();
            List<Vertex> vertices = new List<Vertex>();

            equaterY = hGen.transform.position.y;
            worldRadius = hGen.worldRadius;

            foreach (HexChunk c in hGen.chunks)
            {
                vertices.AddRange(c.vertices);
            }
            
            foreach (Vertex v in vertices)
            {
                List<Vertex> neighbors = new List<Vertex>();

                foreach (Triangle t in hGen.vecTriangleNeighbors[v.pos])
                {
                    if (!neighbors.Contains(t.vA) && t.vA != v)
                    {
                        neighbors.Add(t.vA);
                    }
                    if (!neighbors.Contains(t.vB) && t.vB != v)
                    {
                        neighbors.Add(t.vB);
                    }
                    if (!neighbors.Contains(t.vC) && t.vC != v)
                    {
                        neighbors.Add(t.vC);
                    }
                }

                WindNode n = new WindNode(this, v, neighbors); // Change this, please.
            }
        }
    }
}
