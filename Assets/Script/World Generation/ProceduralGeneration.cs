using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeadReckoning.Procedural
{
    [System.Serializable]
    public class ProceduralSettings
    {
        public Color pineTrunkColor = Color.Lerp(Color.green, Color.red, 0.5f);

        public float pineTrunkWidth = 0.25f;
        public float pineTrunkHeight = 0.25f;
        public float pineLayerReductionRate = 0.75f;
        public float pineLayerWidth = 0.25f;
        public float pineLayerHeight = 0.25f;
        public float pineHeightVariation = 0.05f;
        public float pineWidthtVariation = 0.05f;

    }

    public class ProceduralGeneration
    {
        #region Relative Movement & Axes
        public static Vector3[] FindRelativeAxes(Vector3 v)
        {
            Vector3 forward = v.normalized;

            Vector3 up;

            if (forward == Vector3.forward || forward == Vector3.right)
            {
                up = Vector3.up;
            }
            else if (forward == Vector3.back || forward == Vector3.left)
            {
                up = Vector3.down;
            }
            else if (forward == Vector3.down)
            {
                up = Vector3.right;
            }
            else if (forward == Vector3.up)
            {
                up = Vector3.left;
            }
            else
            {
                up = Vector3.Project(Vector3.up, forward);
                up -= Vector3.up;
                up = up.normalized;
            }

            Vector3 right = -Vector3.Cross(forward, up).normalized;

            return new Vector3[] { forward, up, right };
        }

        public static Vector3 RelativeMovement(Vector3 p, float xMod, float yMod, float zMod, Vector3 forward, Vector3 up, Vector3 right)
        {
            return new Vector3(((p.x * xMod) * right.x) + ((p.y * yMod) * up.x) + ((p.z * zMod) * forward.x), ((p.x * xMod) * right.y) + ((p.y * yMod) * up.y) + ((p.z * zMod) * forward.y), ((p.x * xMod) * right.z) + ((p.y * yMod) * up.z) + ((p.z * zMod) * forward.z));
        }
        #endregion

        public class ProceduralObject
        {
            public Vector3[] vertices;
            public int[] triangles;
            public Color[] colors;
        }

        public class Tree : ProceduralObject
        {
            public enum TreeType { pine, oak, cathedralFig }

            public Tree (ProceduralSettings settings, Vector3 stumpBase, TreeType type, Color canopyColor)
            {
                if (type == TreeType.pine)
                {
                    GeneratePine(settings, stumpBase, canopyColor);
                }
            }

            public void GeneratePine(ProceduralSettings settings, Vector3 stumpBase, Color canopyColor)
            {
                // Each tree will be made up of a base cube...
                // and three pyramids sitting on top of one another.
                // Cube: 8 verts, 12 triangles.
                // Pyramid: 5 verts, 6 triangles.
                // Together: 23 verts, 30 triangles.

                Vector3[] relativeAxes = FindRelativeAxes(stumpBase);

                Vector3 forward = relativeAxes[0];
                Vector3 up = relativeAxes[1];
                Vector3 right = relativeAxes[2];

                float widthVariance = Random.Range(1, 1 + settings.pineWidthtVariation);
                float heightVariance = Random.Range(1, 1 + settings.pineHeightVariation);

                float trunkWidth = settings.pineTrunkWidth;
                float trunkHeight = settings.pineTrunkHeight;
                float layerWidth = settings.pineLayerWidth; // * widthVariance;
                float layerHeight = settings.pineLayerHeight; // * heightVariance;
                float reducRate = settings.pineLayerReductionRate;

                vertices = new Vector3[]
                {
                    // Trunk Base
                    stumpBase + RelativeMovement(pineVertices[0], trunkWidth, trunkWidth, 1, forward, up, right), // A
                    stumpBase + RelativeMovement(pineVertices[1], trunkWidth, trunkWidth, 1, forward, up, right), // B
                    stumpBase + RelativeMovement(pineVertices[2], trunkWidth, trunkWidth, 1, forward, up, right), // C
                    stumpBase + RelativeMovement(pineVertices[3], trunkWidth, trunkWidth, 1, forward, up, right), // D

                    // Trunk Top
                    stumpBase + RelativeMovement(pineVertices[4], trunkWidth, trunkWidth, trunkHeight, forward, up, right), // A
                    stumpBase + RelativeMovement(pineVertices[5], trunkWidth, trunkWidth, trunkHeight, forward, up, right), // B
                    stumpBase + RelativeMovement(pineVertices[6], trunkWidth, trunkWidth, trunkHeight, forward, up, right), // C
                    stumpBase + RelativeMovement(pineVertices[7], trunkWidth, trunkWidth, trunkHeight, forward, up, right), // D

                    // First Pyramid
                    stumpBase + RelativeMovement(pineVertices[8], layerWidth, layerWidth, trunkHeight, forward, up, right), // A
                    stumpBase + RelativeMovement(pineVertices[9], layerWidth, layerWidth, trunkHeight, forward, up, right), // B
                    stumpBase + RelativeMovement(pineVertices[10], layerWidth, layerWidth, trunkHeight, forward, up, right), // C
                    stumpBase + RelativeMovement(pineVertices[11], layerWidth, layerWidth, trunkHeight, forward, up, right), // D
                    stumpBase + RelativeMovement(pineVertices[12], layerWidth, layerWidth, trunkHeight, forward, up, right), // E
                    
                    // Second Pyramid
                    stumpBase + RelativeMovement(pineVertices[13], layerWidth * reducRate, layerWidth * reducRate, layerHeight * 2, forward, up, right), // A
                    stumpBase + RelativeMovement(pineVertices[14], layerWidth * reducRate, layerWidth * reducRate, layerHeight * 2, forward, up, right), // B
                    stumpBase + RelativeMovement(pineVertices[15], layerWidth * reducRate, layerWidth * reducRate, layerHeight * 2, forward, up, right), // C
                    stumpBase + RelativeMovement(pineVertices[16], layerWidth * reducRate, layerWidth * reducRate, layerHeight * 2, forward, up, right), // D
                    stumpBase + RelativeMovement(pineVertices[17], layerWidth * reducRate, layerWidth * reducRate, layerHeight * 2, forward, up, right), // E
                    
                    // Third Pyramid
                    stumpBase + RelativeMovement(pineVertices[18], layerWidth * reducRate * reducRate, layerWidth * reducRate * reducRate, layerHeight * 3, forward, up, right), // A
                    stumpBase + RelativeMovement(pineVertices[19], layerWidth * reducRate * reducRate, layerWidth * reducRate * reducRate, layerHeight * 3, forward, up, right), // B
                    stumpBase + RelativeMovement(pineVertices[20], layerWidth * reducRate * reducRate, layerWidth * reducRate * reducRate, layerHeight * 3, forward, up, right), // C
                    stumpBase + RelativeMovement(pineVertices[21], layerWidth * reducRate * reducRate, layerWidth * reducRate * reducRate, layerHeight * 3, forward, up, right), // D
                    stumpBase + RelativeMovement(pineVertices[22], layerWidth * reducRate * reducRate, layerWidth * reducRate * reducRate, layerHeight * 3, forward, up, right)  // E

                };

                triangles = new int[]
                {
                    0, 1, 2, // Base of trunk
                    2, 1, 3,

                    1, 5, 0, // First trunk side
                    0, 5, 4,

                    3, 7, 1, // Second trunk side
                    1, 7, 5,

                    3, 2, 7, // Third trunk side
                    7, 2, 6,

                    2, 0, 6, // Fourth trunk side
                    6, 0, 4,

                    4, 5, 6, // Top of trunk
                    6, 5, 7,

                    8, 9, 10, // Base of first pyramid
                    10, 9, 11,

                    8, 12, 9, // First pyramid sides
                    9, 12, 11,
                    11, 12, 10,
                    10, 12, 8,

                    13, 14, 15, // Base of second pyramid
                    15, 14, 16,

                    13, 17, 14, // Second pyramid sides
                    14, 17, 16,
                    16, 17, 15,
                    15, 17, 13,

                    18, 19, 20, // Base of third pyramid
                    20, 19, 21,

                    18, 22, 19, // Third pyramid sides
                    19, 22, 21,
                    21, 22, 20,
                    20, 22, 18
                };

                colors = new Color[vertices.Length];

                for (int i = 0; i < 8; i++)
                {
                    colors[i] = settings.pineTrunkColor;
                }
                for (int i = 8; i < 23; i++)
                {
                    colors[i] = canopyColor;
                }
            }

            public static Vector3[] pineVertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, 0f), // Base of trunk.
                new Vector3(-0.5f, 0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),

                new Vector3(-0.5f, -0.5f, 1f), // Top of trunk.
                new Vector3(-0.5f, 0.5f, 1f),
                new Vector3(0.5f, -0.5f, 1f),
                new Vector3(0.5f, -0.5f, 1f),

                new Vector3(-0.5f, -0.5f, 1f), // First pyramid.
                new Vector3(-0.5f, 0.5f, 1f),
                new Vector3(0.5f, -0.5f, 1f),
                new Vector3(0.5f, 0.5f, 1f),
                new Vector3(0, 0, 5), // tippy top #1

                new Vector3(-0.5f, -0.5f, 2f), // Second pyramid.
                new Vector3(-0.5f, 0.5f, 2f),
                new Vector3(0.5f, -0.5f, 2f),
                new Vector3(0.5f, 0.5f, 2f),
                new Vector3(0, 0, 5), // tippy top #2
                
                new Vector3(-0.5f, -0.5f, 3f), // Third pyramid.
                new Vector3(-0.5f, 0.5f, 3f),
                new Vector3(0.5f, -0.5f, 3f),
                new Vector3(0.5f, 0.5f, 3f),
                new Vector3(0, 0, 5), // tippy top #3
            };
        }


    }
}
