using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using DeadReckoning.Procedural;

namespace DeadReckoning.WorldGeneration
{
    public class HexSphereGenerator : MonoBehaviour
    {
        public bool isPlanet;

        public bool generate;
        public bool generatePlanet;
        public bool hexes;

        public int subdivideDepth;
        public float oceanDepth;
        public float worldRadius;

        public GameObject hexChunkPrefab;
        public Material chunkMaterial;
        public GameObject planetPrefab;

        public NoiseSettings noiseSettings;
        public WorldbuildingSettings worldSettings;
        public ProceduralSettings procSettings;

        #region Textures
        public Texture2D[] textureArray;

        private void CreateTextureArray()
        {
            Texture2DArray texture2DArray = new Texture2DArray(textureArray[0].width,
                                                               textureArray[0].height,
                                                               textureArray.Length,
                                                               TextureFormat.RGBA32,
                                                               true, false);

            texture2DArray.filterMode = FilterMode.Bilinear;
            texture2DArray.wrapMode = TextureWrapMode.Repeat;

            for (int i = 0; i < textureArray.Length; i++)
            {
                texture2DArray.SetPixels(textureArray[i].GetPixels(0), i, 0);
            }

            texture2DArray.Apply();

            foreach (HexChunk c in chunks)
            {
                c.gameObject.GetComponent<MeshRenderer>().material.SetTexture("_BiomeTextures", texture2DArray);
            }
        }
        #endregion

        #region Hidden Variables
        [HideInInspector]
        public List<Map.Tile> primitiveTiles;
        [HideInInspector]
        public List<Hex> unsortedHexes;
        [HideInInspector]
        public Hex[,] map;
        [HideInInspector]
        public List<HexChunk> chunks;
        [HideInInspector]
        public Dictionary<Vector3, Hex> vertHexes;
        [HideInInspector]
        public Dictionary<Vector3, List<Triangle>> vecTriangleNeighbors;
        [HideInInspector]
        NoiseFilter noiseFilter;
        [HideInInspector]
        public float maxContDist;
        [HideInInspector]
        public float oceanRadius;

        Camera mainCamera;
        #endregion

        #region General Generation
        void Start()
        {
            // Starts the generation process (if that's something we want).

            if (generate)
            {
                Stopwatch gt = new Stopwatch();
                gt.Start();

                Generate();

                gt.Stop();
                UnityEngine.Debug.Log($"Ocean generation took {gt.ElapsedMilliseconds} milliseconds.");
            }

            if (generatePlanet)
            {
                Stopwatch pt = new Stopwatch();
                pt.Start();

                GeneratePlanet();

                pt.Stop();
                UnityEngine.Debug.Log($"Planet generation took {pt.ElapsedMilliseconds} milliseconds.");
            }
        }

        public void Generate()
        {
            // Generates the world.
            mainCamera = Camera.main;
            vertHexes = new Dictionary<Vector3, Hex>();
            noiseFilter = new NoiseFilter(noiseSettings);
            unsortedHexes = new List<Hex>();
            primitiveTiles = new List<Map.Tile>();

            for (int i = 0; i < icoTris.Length; i += 3)
            {
                // Sets up all the necessary chunks and their values.

                GameObject g = Instantiate(hexChunkPrefab, this.transform.position, Quaternion.identity, this.gameObject.transform);
                g.GetComponent<MeshRenderer>().material = chunkMaterial;

                HexChunk c = g.GetComponent<HexChunk>();

                c.triangles = new Triangle[1];
                c.triangles[0] = new Triangle(new Vertex(icoVerts[icoTris[i]]), new Vertex(icoVerts[icoTris[i + 1]]), new Vertex(icoVerts[icoTris[i + 2]]));

                c.origin = c.triangles[0];

                // c.neighbors = c.FindNeighbors(chunks.Count);
                // We no longer need to find neighbors, so this won't be necessary.

                c.number = chunks.Count;
                c.color = Color.white;

                if (!isPlanet)
                {
                    c.gameObject.GetComponent<MeshCollider>().enabled = false;
                }

                chunks.Add(c);
            }

            if (isPlanet)
            {
                CreateTextureArray();
            }

            for (int d = 0; d < subdivideDepth; d++)
            {
                // Calls each chunk's subdivision method.

                foreach (HexChunk c in chunks)
                {
                    c.Subdivide(worldRadius);
                }
            }

            if (hexes)
            {
                // This should only ever be called right before making hexes.
                FindNeighbors();

                foreach (HexChunk c in chunks)
                {
                    c.Hexify(this);
                }

                foreach (HexChunk c in chunks)
                {
                    c.SortHexNeighbors();

                    c.Render(true);
                }
            }
            else
            {
                if (isPlanet)
                {
                    FindNeighbors();

                    foreach (HexChunk c in chunks)
                    {
                        c.Hexify(this);
                    }

                    foreach (HexChunk c in chunks)
                    {
                        c.SortHexNeighbors();
                        c.CreateMapTiles(this);
                    }

                    Map.TileMap map = new Map.TileMap(primitiveTiles, worldSettings);
                    map.DetermineContinents(this);
                    UnityEngine.Debug.Log($"{primitiveTiles.Count} tiles generated.");

                    foreach (HexChunk c in chunks)
                    {
                        c.AddHexNoise(noiseFilter, noiseSettings, this);
                    }

                    map.Generate(this);

                    foreach (HexChunk c in chunks)
                    {
                        c.SpawnObjects(this);
                        c.Render(true);
                    }
                }
                else
                {
                    foreach (HexChunk c in chunks)
                    {
                        c.Render(false);
                    }
                }
            }
        }

        public void GeneratePlanet()
        {
            GameObject g = Instantiate(planetPrefab, this.gameObject.transform.position, Quaternion.identity, this.transform);

            HexSphereGenerator planetGenerator = g.GetComponent<HexSphereGenerator>();

            planetGenerator.worldRadius = worldRadius * oceanDepth;

            planetGenerator.worldSettings = worldSettings;
            planetGenerator.procSettings = procSettings;

            if (!planetGenerator.hexes)
            {
                planetGenerator.noiseSettings = noiseSettings;
            }

            planetGenerator.isPlanet = true;
            planetGenerator.maxContDist = maxContDist;
            planetGenerator.oceanRadius = worldRadius;

            planetGenerator.textureArray = textureArray;

            planetGenerator.Generate();

            foreach (HexChunk c in planetGenerator.chunks)
            {
                c.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }
        #endregion

        #region Presentation & Details

        public void ChangeLenses()
        {
            foreach (HexChunk c in chunks)
            {
                c.UpdateColors(mainCamera.gameObject.GetComponent<GlobeCamera>().lens);
            }
        }
        #endregion

        #region Finding Neighbors
        public void FindNeighbors()
        {
            // Finds all the neighbors to a vertex (for making hexes, afterwards).

            vecTriangleNeighbors = new Dictionary<Vector3, List<Triangle>>();

            foreach (HexChunk c in chunks)
            {
                for (int i = 0; i < c.triangles.Length; i++)
                {
                    Triangle t = c.triangles[i];

                    if (vecTriangleNeighbors.ContainsKey(t.vA.pos))
                    {
                        if (!vecTriangleNeighbors[t.vA.pos].Contains(t))
                        {
                            vecTriangleNeighbors[t.vA.pos].Add(t);
                        }
                    }
                    else
                    {
                        vecTriangleNeighbors.Add(t.vA.pos, new List<Triangle>());
                        vecTriangleNeighbors[t.vA.pos].Add(t);
                    }

                    if (vecTriangleNeighbors.ContainsKey(t.vB.pos))
                    {
                        if (!vecTriangleNeighbors[t.vB.pos].Contains(t))
                        {
                            vecTriangleNeighbors[t.vB.pos].Add(t);
                        }
                    }
                    else
                    {
                        vecTriangleNeighbors.Add(t.vB.pos, new List<Triangle>());
                        vecTriangleNeighbors[t.vB.pos].Add(t);
                    }

                    if (vecTriangleNeighbors.ContainsKey(t.vC.pos))
                    {
                        if (!vecTriangleNeighbors[t.vC.pos].Contains(t))
                        {
                            vecTriangleNeighbors[t.vC.pos].Add(t);
                        }
                    }
                    else
                    {
                        vecTriangleNeighbors.Add(t.vC.pos, new List<Triangle>());
                        vecTriangleNeighbors[t.vC.pos].Add(t);
                    }
                }
            }
        }
        #endregion

        #region Icosahedron Data

        static float t = 100.0f;
        static float r = t * 0.89441592209664961889545770584312f;
        static float q = 0.447215f;

        Vector3[] icoVerts = new Vector3[] // 12
        {
        new Vector3 (0,
            t,
            0),
        new Vector3 (r * Mathf.Cos(0),
            t * q,
            r * Mathf.Sin(0)),
        new Vector3 (r * Mathf.Cos(Mathf.Deg2Rad*72),
            t * q,
            r * Mathf.Sin(Mathf.Deg2Rad * 72)),
        new Vector3 (r * Mathf.Cos(Mathf.Deg2Rad * 72 * 2),
            t * q,
            r * Mathf.Sin(Mathf.Deg2Rad * 72 * 2)),
        new Vector3 (r * Mathf.Cos(Mathf.Deg2Rad * 72 * 3),
            t * q,
            r * Mathf.Sin(Mathf.Deg2Rad * 72 * 3)),
        new Vector3(r * Mathf.Cos(Mathf.Deg2Rad * 72 * 4),
            t * q,
            r * Mathf.Sin(Mathf.Deg2Rad * 72 * 4)),

        new Vector3 (r * Mathf.Cos(Mathf.Deg2Rad * 36),
            -t * q,
            r * Mathf.Sin(Mathf.Deg2Rad * 36)),
        new Vector3(r * Mathf.Cos(Mathf.Deg2Rad * 108),
            -t * q,
            r * Mathf.Sin(Mathf.Deg2Rad * 108)),
        new Vector3(r * Mathf.Cos(Mathf.Deg2Rad * (72 * 2 + 36)),
            -t * q,
            r * Mathf.Sin(Mathf.Deg2Rad * (72 * 2 + 36))),
        new Vector3 (r * Mathf.Cos(Mathf.Deg2Rad * (72 * 3 + 36)),
            -t * q,
            r * Mathf.Sin(Mathf.Deg2Rad * (72 * 3 + 36))),
        new Vector3(r * Mathf.Cos(Mathf.Deg2Rad * (72 * 4 + 36)),
            -t * q,
            r * Mathf.Sin(Mathf.Deg2Rad * (72 * 4 + 36))),

        new Vector3 (0, -t, 0)
        };

        int[] icoTris = new int[] { // 60
        0,2,1,  // 1
        0,3,2,
        0,4,3,
        0,5,4,
        0,1,5,

        1,2,6, // 6
        2,7,6,
        2,3,7,
        3,8,7,
        3,4,8,
        4,9,8,
        4,5,9,
        5,10,9,
        5,1,10,
        1,6,10,

        11,6,7, // 16
        11,7,8,
        11,8,9,
        11,9,10,
        11,10,6 // 20
    };

        #endregion

        #region No Longer Implemented
        //public void CoordinateHexes()
        //{
        //    // This is broken.

        //    // Takes the unsorted hexes' coordinates and puts them...
        //    // in the map array for safe keeping.

        //    List<Hex> coordinatedHexes = new List<Hex>();

        //    RaycastHit hit;
        //    Physics.Raycast(transform.position + new Vector3(0, -worldRadius * 2, 0), transform.TransformDirection(Vector3.up) * 1000.0f, out hit, Mathf.Infinity);

        //    Hex selectHex = hit.transform.gameObject.GetComponent<HexChunk>().ListNearestHex(hit.point);

        //    map = new Hex[Mathf.RoundToInt(10 * Mathf.Pow(2, subdivideDepth)), 10 * Mathf.RoundToInt(Mathf.Pow(2, subdivideDepth))];

        //    Camera cam = Camera.main;

        //    cam.gameObject.transform.GetChild(0).gameObject.GetComponent<LineRenderer>().SetPosition(0, this.transform.position);
        //    cam.gameObject.transform.GetChild(0).gameObject.GetComponent<LineRenderer>().SetPosition(1, selectHex.center.pos * 2);

        //    while (coordinatedHexes.Count < vertHexes.Count)
        //    {
        //        coordinatedHexes.Add(selectHex);
        //        map[selectHex.x, selectHex.y] = selectHex;

        //        if (!coordinatedHexes.Contains(selectHex.neighbors[2]))
        //        {
        //            selectHex.neighbors[2].x = selectHex.x + 1;
        //            selectHex.neighbors[2].y = selectHex.y;

        //            selectHex = selectHex.neighbors[2];
        //        }
        //        else
        //        {
        //            selectHex.neighbors[1].x = selectHex.x;
        //            selectHex.neighbors[1].y = selectHex.y + 1;

        //            selectHex = selectHex.neighbors[1];
        //        }
        //    }
        //}
        #endregion
    }

    public class Vertex
    {
        public int x;
        public int y;

        public Vector3 pos;

        public Vertex(Vector3 v)
        {
            pos = v;
        }

        public void Normalize(float r)
        {
            pos = pos.normalized * r;
        }

    }

    public class Hex
    {
        public Map.Tile tile;
        public HexChunk chunk;

        public int x;
        public int y;

        public bool pent;

        public Vertex center;

        public Vertex[] vertices;

        public Vector2 uv;
        public Color color;
        public Color windColor;
        public Color currentColor;
        public Color biomeColor;
        public Color temperatureColor;
        public Color precipitationColor;

        public List<Hex> neighbors = new List<Hex>();

        public void AddNeighbor(Hex h)
        {
            if (!neighbors.Contains(h))
            {
                neighbors.Add(h);
            }

            if (!h.neighbors.Contains(this))
            {
                h.neighbors.Add(this);
            }
        }

        public void CalculateCenter()
        {
            Vector3 cU = new Vector3(0, 0, 0);

            for (int i = 0; i < vertices.Length; i++)
            {
                cU += vertices[i].pos;
            }

            center = new Vertex(cU / vertices.Length);
        }
    }

    public class Triangle
    {
        public Vertex vA;
        public Vertex vB;
        public Vertex vC;

        public Triangle(Vertex a, Vertex b, Vertex c)
        {
            vA = a;
            vB = b;
            vC = c;
        }

        public void NormalizeVertices(float radius)
        {
            vA.Normalize(radius);
            vB.Normalize(radius);
            vC.Normalize(radius);
        }
    }
}