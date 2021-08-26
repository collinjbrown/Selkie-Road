using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DeadReckoning.Procedural;
using DeadReckoning.Map;

namespace DeadReckoning.WorldGeneration
{
    public class HexChunk : MonoBehaviour
    {
        public int number;

        public int[] neighbors;

        public Vertex[] vertices;
        public Triangle[] triangles;
        public Hex[] hexes;

        public Dictionary<Vector3, Hex> hexCenters;

        public Color color;

        public Triangle origin;

        public GameObject proceduralPrefab;
        public GameObject structureClusterPrefab;
        public GameObject grassPrefab;

        public List<GameObject> structureClusters = new List<GameObject>();

        #region Hidden Variables
        [HideInInspector]
        public Vector3[] mapVerts;

        [HideInInspector]
        public int[] mapTris;

        [HideInInspector]
        public Vector2[] mapUV;
        [HideInInspector]
        public Vector2[] randomUV;
        [HideInInspector]
        public Vector2[] biomeUV;
        [HideInInspector]
        public Vector2[] dummyUV;
        [HideInInspector]
        public Color[] mapColors;
        [HideInInspector]
        public Color[] windColors;
        [HideInInspector]
        public Color[] currentColors;
        [HideInInspector]
        public Color[] biomeColors;
        [HideInInspector]
        public Color[] temperatureColors;
        [HideInInspector]
        public Color[] precipitationColors;
        [HideInInspector]
        public Color[] plateColors;
        [HideInInspector]
        public Color[] countyColors;
        [HideInInspector]
        public Color[] nationColors;

        [HideInInspector]
        public Vector3[] wallVerts;

        [HideInInspector]
        public int[] wallTris;
        #endregion

        #region Rendering
        public void Render(bool hex)
        {
            // Displays the chunk in the world.

            MapVertsAndTris(hex);

            Mesh mesh = this.gameObject.GetComponent<MeshFilter>().mesh;

            mesh.Clear();
            mesh.vertices = mapVerts;
            mesh.triangles = mapTris;

            if (hex)
            {
                mesh.SetUVs(0, mapUV);
                mesh.SetUVs(2, biomeUV);
                mesh.SetUVs(3, randomUV);
                mesh.colors = mapColors;
            }

            mesh.Optimize();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            MeshCollider meshCol = this.gameObject.GetComponent<MeshCollider>();
            meshCol.sharedMesh = mesh;
        }

        public void UpdateColors(GlobeCamera.Lens lens)
        {
            Mesh mesh = this.gameObject.GetComponent<MeshFilter>().mesh;

            mesh.Clear();
            mesh.vertices = mapVerts;
            mesh.triangles = mapTris;

            if (lens == GlobeCamera.Lens.plain)
            {
                foreach (ProceduralContainer g in this.gameObject.GetComponentsInChildren<ProceduralContainer>(true))
                {
                    if (g.gameObject != this)
                    {
                        g.gameObject.SetActive(true);
                    }
                }
            }
            else
            {
                foreach (ProceduralContainer g in this.gameObject.GetComponentsInChildren<ProceduralContainer>(true))
                {
                    if (g.gameObject != this)
                    {
                        g.gameObject.SetActive(false);
                    }
                }
            }

            if (lens == GlobeCamera.Lens.plain)
            {
                mesh.SetUVs(0, mapUV);
                mesh.SetUVs(2, biomeUV);
                mesh.SetUVs(3, randomUV);
                mesh.colors = mapColors;
            }
            else if (lens == GlobeCamera.Lens.matte)
            {
                mesh.SetUVs(2, dummyUV);
                mesh.SetUVs(3, dummyUV);
                mesh.colors = mapColors;
            }
            else if (lens == GlobeCamera.Lens.winds)
            {
                mesh.SetUVs(2, dummyUV);
                mesh.SetUVs(3, randomUV);
                mesh.colors = windColors;
            }
            else if (lens == GlobeCamera.Lens.currents)
            {
                mesh.SetUVs(2, dummyUV);
                mesh.SetUVs(3, randomUV);
                mesh.colors = currentColors;
            }
            else if (lens == GlobeCamera.Lens.biomes)
            {
                mesh.SetUVs(2, dummyUV);
                mesh.SetUVs(3, randomUV);
                mesh.colors = biomeColors;
            }
            else if (lens == GlobeCamera.Lens.temperature)
            {
                mesh.SetUVs(2, dummyUV);
                mesh.SetUVs(3, randomUV);
                mesh.colors = temperatureColors;
            }
            else if (lens == GlobeCamera.Lens.precipitation)
            {
                mesh.SetUVs(2, dummyUV);
                mesh.SetUVs(3, randomUV);
                mesh.colors = precipitationColors;
            }
            else if (lens == GlobeCamera.Lens.plates)
            {
                mesh.SetUVs(2, dummyUV);
                mesh.SetUVs(3, randomUV);
                mesh.colors = plateColors;
            }
            else if (lens == GlobeCamera.Lens.counties)
            {
                mesh.SetUVs(2, dummyUV);
                mesh.SetUVs(3, randomUV);
                mesh.colors = countyColors;
            }
            else if (lens == GlobeCamera.Lens.nations)
            {
                UpdateNations();

                mesh.SetUVs(2, dummyUV);
                mesh.SetUVs(3, randomUV);
                mesh.colors = nationColors;
            }

            // mesh.Optimize();
            mesh.RecalculateNormals();
            // mesh.RecalculateBounds();
        }

        public void UpdateNations()
        {
            int vMod = 0;
            int tMod = 0;

            if (wallVerts.Length > 0)
            {
                vMod = wallVerts.Length;
                tMod = wallTris.Length;
            }

            int vertsPerHex = 12 + 1;

            nationColors = new Color[mapVerts.Length];

            int vertOffset = 0;

            for (int t = 0; t < hexes.Length; t++)
            {
                Hex h = hexes[t];

                for (int i = 0; i < 10; i++)
                {
                    if (h.tile.county != null)
                    {
                        countyColors[(t * vertsPerHex) + i + vertOffset] = h.tile.county.color;

                        if (h.tile.county.civ != null)
                        {
                            nationColors[(t * vertsPerHex) + i + vertOffset] = h.tile.county.civ.color;
                        }
                        else
                        {
                            nationColors[(t * vertsPerHex) + i + vertOffset] = Color.white;
                        }
                    }
                    else
                    {
                        countyColors[(t * vertsPerHex) + i + vertOffset] = Color.white;
                        nationColors[(t * vertsPerHex) + i + vertOffset] = Color.white;
                    }
                }

                if (!h.pent)
                {
                    for (int i = 10; i < vertsPerHex; i++)
                    {
                        if (h.tile.county != null)
                        {
                            countyColors[(t * vertsPerHex) + i + vertOffset] = h.tile.county.color;

                            if (h.tile.county.civ != null)
                            {
                                nationColors[(t * vertsPerHex) + i + vertOffset] = h.tile.county.civ.color;
                            }
                            else
                            {
                                nationColors[(t * vertsPerHex) + i + vertOffset] = Color.white;
                            }
                        }
                        else
                        {
                            countyColors[(t * vertsPerHex) + i + vertOffset] = Color.white;
                            nationColors[(t * vertsPerHex) + i + vertOffset] = Color.white;
                        }
                    }

                    vertOffset = 0;
                }
                else
                {
                    if (h.tile.county != null)
                    {
                        countyColors[(t * vertsPerHex) + 10 + vertOffset] = h.tile.county.color;

                        if (h.tile.county.civ != null)
                        {
                            nationColors[(t * vertsPerHex) + 10 + vertOffset] = h.tile.county.civ.color;
                        }
                        else
                        {
                            nationColors[(t * vertsPerHex) + 10 + vertOffset] = Color.white;
                        }
                    }
                    else
                    {
                        countyColors[(t * vertsPerHex) + 10 + vertOffset] = Color.white;
                        nationColors[(t * vertsPerHex) + 10 + vertOffset] = Color.white;
                    }

                    vertOffset = -2;
                }
            }
        }

        public void MapVertsAndTris(bool hex)
        {
            // Takes a list of hexes or triangles and maps them onto arrays...
            // that are readable by the mesh renderer.
            // This shouldn't really be called except right before rendering.
            // Hence why it is called in that method.

            if (!hex)
            {
                List<Triangle> trisToMap = new List<Triangle>(triangles);

                mapVerts = new Vector3[trisToMap.Count * 3];
                mapTris = new int[trisToMap.Count * 3];

                for (int t = 0; t < trisToMap.Count; t++)
                {
                    Triangle tri = trisToMap[t];

                    mapVerts[t * 3] = tri.vA.pos; // A
                    mapTris[t * 3] = t * 3;

                    mapVerts[t * 3 + 1] = tri.vB.pos; // B
                    mapTris[t * 3 + 1] = t * 3 + 1;

                    mapVerts[t * 3 + 2] = tri.vC.pos; // C
                    mapTris[t * 3 + 2] = t * 3 + 2;

                }
            }
            else
            {

                int vMod = 0;
                int tMod = 0;

                if (wallVerts.Length > 0)
                {
                    vMod = wallVerts.Length;
                    tMod = wallTris.Length;
                }

                int vertsPerHex = 12 + 1;
                int trisPerHex = 12 * 3;

                mapVerts = new Vector3[(hexes.Length * vertsPerHex) + vMod];
                mapTris = new int[(hexes.Length * trisPerHex) + tMod];
                mapUV = new Vector2[mapVerts.Length];
                randomUV = new Vector2[mapVerts.Length];
                biomeUV = new Vector2[mapVerts.Length];
                dummyUV = new Vector2[mapVerts.Length];
                mapColors = new Color[mapVerts.Length];
                windColors = new Color[mapVerts.Length];
                currentColors = new Color[mapVerts.Length];
                biomeColors = new Color[mapVerts.Length];
                temperatureColors = new Color[mapVerts.Length];
                precipitationColors = new Color[mapVerts.Length];
                plateColors = new Color[mapVerts.Length];
                countyColors = new Color[mapVerts.Length];
                nationColors = new Color[mapVerts.Length];

                for (int dum = 0; dum < dummyUV.Length; dum++) // This is used when we want to show colors rather than textures.
                {
                    dummyUV[0] = new Vector2(0, 0);
                }

                int vertOffset = 0; // Used to make sure there aren't gaps in the tri array due to pentagons.
                int triOffset = 0;

                for (int t = 0; t < hexes.Length; t++)
                {
                    Hex h = hexes[t];

                    mapVerts[(t * vertsPerHex) + 0 + vertOffset] = h.center.pos;                                      // A
                    mapVerts[(t * vertsPerHex) + 1 + vertOffset] = h.vertices[0].pos;                                // B
                    mapVerts[(t * vertsPerHex) + 2 + vertOffset] = h.vertices[1].pos;                                // C
                    mapVerts[(t * vertsPerHex) + 3 + vertOffset] = (h.vertices[0].pos + h.vertices[1].pos) / 2.0f;  // BC
                    mapVerts[(t * vertsPerHex) + 4 + vertOffset] = h.vertices[2].pos;                                // D
                    mapVerts[(t * vertsPerHex) + 5 + vertOffset] = (h.vertices[1].pos + h.vertices[2].pos) / 2.0f;  // CD
                    mapVerts[(t * vertsPerHex) + 6 + vertOffset] = h.vertices[3].pos;                                // E
                    mapVerts[(t * vertsPerHex) + 7 + vertOffset] = (h.vertices[2].pos + h.vertices[3].pos) / 2.0f;  // DE
                    mapVerts[(t * vertsPerHex) + 8 + vertOffset] = h.vertices[4].pos;                                // F
                    mapVerts[(t * vertsPerHex) + 9 + vertOffset] = (h.vertices[3].pos + h.vertices[4].pos) / 2.0f;  // EF

                    mapUV[(t * vertsPerHex) + 0 + vertOffset] = (new Vector2(0.5f, 0.5f) + new Vector2(1.0f, 0.5f) * 0.5f);                            // A
                    mapUV[(t * vertsPerHex) + 1 + vertOffset] = (new Vector2(-0.5f, Mathf.Sqrt(3) / 2.0f) + new Vector2(1.0f, 0.5f) * 0.5f);           // B
                    mapUV[(t * vertsPerHex) + 2 + vertOffset] = (new Vector2(0.5f, Mathf.Sqrt(3) / 2.0f) + new Vector2(1.0f, 0.5f) * 0.5f);            // C
                    mapUV[(t * vertsPerHex) + 3 + vertOffset] = (new Vector2(0, Mathf.Sqrt(3) / 2.0f) + new Vector2(1.0f, 0.5f) * 0.5f);               // BC
                    mapUV[(t * vertsPerHex) + 4 + vertOffset] = (new Vector2(1.0f, 0) + new Vector2(1.0f, 0.5f) * 0.5f);                               // D
                    mapUV[(t * vertsPerHex) + 5 + vertOffset] = (Vector2.Lerp(new Vector2(1.0f, 0), new Vector2(0.5f, -Mathf.Sqrt(3) / 2.0f), 0.5f) + new Vector2(1.0f, 0.5f) * 0.5f); // CD
                    mapUV[(t * vertsPerHex) + 6 + vertOffset] = (new Vector2(0.5f, -Mathf.Sqrt(3) / 2.0f) + new Vector2(1.0f, 0.5f) * 0.5f);           // E
                    mapUV[(t * vertsPerHex) + 7 + vertOffset] = (new Vector2(0, -Mathf.Sqrt(3) / 2.0f) + new Vector2(1.0f, 0.5f) * 0.5f);              // DE
                    mapUV[(t * vertsPerHex) + 8 + vertOffset] = (new Vector2(-0.5f, -Mathf.Sqrt(3) / 2.0f) + new Vector2(1.0f, 0.5f) * 0.5f);          // F
                    mapUV[(t * vertsPerHex) + 9 + vertOffset] = (Vector2.Lerp(new Vector2(0.5f, -Mathf.Sqrt(3) / 2.0f), new Vector2(-0.5f, -Mathf.Sqrt(3) / 2.0f), 0.5f) + new Vector2(1.0f, 0.5f) * 0.5f); // EF

                    float variance = Random.Range(0, 0.5f);
                    for (int i = 0; i < 10; i++)
                    {
                        randomUV[(t * vertsPerHex) + i + vertOffset] = new Vector2(variance, 0);
                        biomeUV[(t * vertsPerHex) + i + vertOffset] = h.uv;
                        mapColors[(t * vertsPerHex) + i + vertOffset] = h.color;
                        windColors[(t * vertsPerHex) + i + vertOffset] = h.windColor;
                        currentColors[(t * vertsPerHex) + i + vertOffset] = h.currentColor;
                        biomeColors[(t * vertsPerHex) + i + vertOffset] = h.biomeColor;
                        precipitationColors[(t * vertsPerHex) + i + vertOffset] = h.precipitationColor;
                        temperatureColors[(t * vertsPerHex) + i + vertOffset] = h.temperatureColor;
                        plateColors[(t * vertsPerHex) + i + vertOffset] = h.tile.plate.color;

                        if (h.tile.county != null)
                        {
                            countyColors[(t * vertsPerHex) + i + vertOffset] = h.tile.county.color;

                            if (h.tile.county.civ != null)
                            {
                                nationColors[(t * vertsPerHex) + i + vertOffset] = h.tile.county.civ.color;
                            }
                            else
                            {
                                nationColors[(t * vertsPerHex) + i + vertOffset] = Color.white;
                            }
                        }
                        else
                        {
                            countyColors[(t * vertsPerHex) + i + vertOffset] = Color.white;
                            nationColors[(t * vertsPerHex) + i + vertOffset] = Color.white;
                        }
                    }

                    // Triangle One (A - BC - B)
                    mapTris[(t * trisPerHex) + 0 + triOffset] = (t * vertsPerHex) + 0 + vertOffset;
                    mapTris[(t * trisPerHex) + 1 + triOffset] = (t * vertsPerHex) + 3 + vertOffset;
                    mapTris[(t * trisPerHex) + 2 + triOffset] = (t * vertsPerHex) + 1 + vertOffset;

                    // Triangle Two (A - C - BC)
                    mapTris[(t * trisPerHex) + 3 + triOffset] = (t * vertsPerHex) + 0 + vertOffset;
                    mapTris[(t * trisPerHex) + 4 + triOffset] = (t * vertsPerHex) + 2 + vertOffset;
                    mapTris[(t * trisPerHex) + 5 + triOffset] = (t * vertsPerHex) + 3 + vertOffset;

                    // Triangle Three (A - CD - C)
                    mapTris[(t * trisPerHex) + 6 + triOffset] = (t * vertsPerHex) + 0 + vertOffset;
                    mapTris[(t * trisPerHex) + 7 + triOffset] = (t * vertsPerHex) + 5 + vertOffset;
                    mapTris[(t * trisPerHex) + 8 + triOffset] = (t * vertsPerHex) + 2 + vertOffset;

                    // Triangle Four (A - D - CD)
                    mapTris[(t * trisPerHex) + 9 + triOffset] = (t * vertsPerHex) + 0 + vertOffset;
                    mapTris[(t * trisPerHex) + 10 + triOffset] = (t * vertsPerHex) + 4 + vertOffset;
                    mapTris[(t * trisPerHex) + 11 + triOffset] = (t * vertsPerHex) + 5 + vertOffset;

                    // Triangle Five (A - DE - D)
                    mapTris[(t * trisPerHex) + 12 + triOffset] = (t * vertsPerHex) + 0 + vertOffset;
                    mapTris[(t * trisPerHex) + 13 + triOffset] = (t * vertsPerHex) + 7 + vertOffset;
                    mapTris[(t * trisPerHex) + 14 + triOffset] = (t * vertsPerHex) + 4 + vertOffset;

                    // Triangle Six (A - E - DE)
                    mapTris[(t * trisPerHex) + 15 + triOffset] = (t * vertsPerHex) + 0 + vertOffset;
                    mapTris[(t * trisPerHex) + 16 + triOffset] = (t * vertsPerHex) + 6 + vertOffset;
                    mapTris[(t * trisPerHex) + 17 + triOffset] = (t * vertsPerHex) + 7 + vertOffset;

                    // Triangle Seven (A - EF - E)
                    mapTris[(t * trisPerHex) + 18 + triOffset] = (t * vertsPerHex) + 0 + vertOffset;
                    mapTris[(t * trisPerHex) + 19 + triOffset] = (t * vertsPerHex) + 9 + vertOffset;
                    mapTris[(t * trisPerHex) + 20 + triOffset] = (t * vertsPerHex) + 6 + vertOffset;

                    // Triangle Eight (A - F - EF)
                    mapTris[(t * trisPerHex) + 21 + triOffset] = (t * vertsPerHex) + 0 + vertOffset;
                    mapTris[(t * trisPerHex) + 22 + triOffset] = (t * vertsPerHex) + 8 + vertOffset;
                    mapTris[(t * trisPerHex) + 23 + triOffset] = (t * vertsPerHex) + 9 + vertOffset;

                    // Recheck listings.
                    if (!h.pent)
                    {
                        mapVerts[(t * vertsPerHex) + 10 + vertOffset] = h.vertices[5].pos;                                // G
                        mapVerts[(t * vertsPerHex) + 11 + vertOffset] = (h.vertices[4].pos + h.vertices[5].pos) / 2.0f;  // FG
                        mapVerts[(t * vertsPerHex) + 12 + vertOffset] = (h.vertices[5].pos + h.vertices[0].pos) / 2.0f;  // GB

                        mapUV[(t * vertsPerHex) + 10 + vertOffset] = (new Vector2(-1, 0) + new Vector2(1, 0.5f)) * 0.5f;
                        mapUV[(t * vertsPerHex) + 11 + vertOffset] = (Vector2.Lerp(new Vector2(-1, 0), new Vector2(-0.5f, -Mathf.Sqrt(3) / 2.0f), 0.5f) + new Vector2(1, 0.5f)) * 0.5f;
                        mapUV[(t * vertsPerHex) + 12 + vertOffset] = (Vector2.Lerp(new Vector2(-1, 0), new Vector2(-0.5f, Mathf.Sqrt(3) / 2.0f), 0.5f) + new Vector2(1, 0.5f)) * 0.5f;

                        for (int i = 10; i < vertsPerHex; i++)
                        {
                            biomeUV[(t * vertsPerHex) + i + vertOffset] = h.uv;
                            randomUV[(t * vertsPerHex) + i + vertOffset] = new Vector2(variance, 0);
                            mapColors[(t * vertsPerHex) + i + vertOffset] = h.color;
                            windColors[(t * vertsPerHex) + i + vertOffset] = h.windColor;
                            currentColors[(t * vertsPerHex) + i + vertOffset] = h.currentColor;
                            biomeColors[(t * vertsPerHex) + i + vertOffset] = h.biomeColor;
                            temperatureColors[(t * vertsPerHex) + i + vertOffset] = h.temperatureColor;
                            precipitationColors[(t * vertsPerHex) + i + vertOffset] = h.precipitationColor;
                            plateColors[(t * vertsPerHex) + i + vertOffset] = h.tile.plate.color;

                            if (h.tile.county != null)
                            {
                                countyColors[(t * vertsPerHex) + i + vertOffset] = h.tile.county.color;

                                if (h.tile.county.civ != null)
                                {
                                    nationColors[(t * vertsPerHex) + i + vertOffset] = h.tile.county.civ.color;
                                }
                                else
                                {
                                    nationColors[(t * vertsPerHex) + i + vertOffset] = Color.white;
                                }
                            }
                            else
                            {
                                countyColors[(t * vertsPerHex) + i + vertOffset] = Color.white;
                                nationColors[(t * vertsPerHex) + i + vertOffset] = Color.white;
                            }
                        }

                        // Triangle Nine (A - FG - F)
                        mapTris[(t * trisPerHex) + 24 + triOffset] = (t * vertsPerHex) + 0 + vertOffset;
                        mapTris[(t * trisPerHex) + 25 + triOffset] = (t * vertsPerHex) + 11 + vertOffset;
                        mapTris[(t * trisPerHex) + 26 + triOffset] = (t * vertsPerHex) + 8 + vertOffset;

                        // Triangle Ten (A - G - FG)
                        mapTris[(t * trisPerHex) + 27 + triOffset] = (t * vertsPerHex) + 0 + vertOffset;
                        mapTris[(t * trisPerHex) + 28 + triOffset] = (t * vertsPerHex) + 10 + vertOffset;
                        mapTris[(t * trisPerHex) + 29 + triOffset] = (t * vertsPerHex) + 11 + vertOffset;

                        // Triangle Eleven (A - GB - G)
                        mapTris[(t * trisPerHex) + 30 + triOffset] = (t * vertsPerHex) + 0 + vertOffset;
                        mapTris[(t * trisPerHex) + 31 + triOffset] = (t * vertsPerHex) + 12 + vertOffset;
                        mapTris[(t * trisPerHex) + 32 + triOffset] = (t * vertsPerHex) + 10 + vertOffset;

                        // Triangle Twelve (A - B - GB)
                        mapTris[(t * trisPerHex) + 33 + triOffset] = (t * vertsPerHex) + 0 + vertOffset;
                        mapTris[(t * trisPerHex) + 34 + triOffset] = (t * vertsPerHex) + 1 + vertOffset;
                        mapTris[(t * trisPerHex) + 35 + triOffset] = (t * vertsPerHex) + 12 + vertOffset;

                        vertOffset = 0;
                        triOffset = 0;
                    }
                    else
                    {
                        mapVerts[(t * vertsPerHex) + 10 + vertOffset] = (h.vertices[4].pos + h.vertices[0].pos) / 2.0f;  // FB

                        mapUV[(t * vertsPerHex) + 10 + vertOffset] = Vector2.Lerp(new Vector2(-0.5f, -Mathf.Sqrt(3) / 2.0f), new Vector2(-0.5f, Mathf.Sqrt(3) / 2.0f), 0.5f);

                        biomeUV[(t * vertsPerHex) + 10 + vertOffset] = h.uv;
                        randomUV[(t * vertsPerHex) + 10 + vertOffset] = new Vector2(variance, 0);
                        mapColors[(t * vertsPerHex) + 10 + vertOffset] = h.color;
                        windColors[(t * vertsPerHex) + 10 + vertOffset] = h.windColor;
                        currentColors[(t * vertsPerHex) + 10 + vertOffset] = h.currentColor;
                        biomeColors[(t * vertsPerHex) + 10 + vertOffset] = h.biomeColor;
                        precipitationColors[(t * vertsPerHex) + 10 + vertOffset] = h.precipitationColor;
                        temperatureColors[(t * vertsPerHex) + 10 + vertOffset] = h.temperatureColor;

                        if (h.tile.plate != null)
                        {
                            plateColors[(t * vertsPerHex) + 10 + vertOffset] = h.tile.plate.color;
                        }
                        else
                        {
                            plateColors[(t * vertsPerHex) + 10 + vertOffset] = Color.white;
                        }

                        if (h.tile.county != null)
                        {
                            countyColors[(t * vertsPerHex) + 10 + vertOffset] = h.tile.county.color;

                            if (h.tile.county.civ != null)
                            {
                                nationColors[(t * vertsPerHex) + 10 + vertOffset] = h.tile.county.civ.color;
                            }
                            else
                            {
                                nationColors[(t * vertsPerHex) + 10 + vertOffset] = Color.white;
                            }
                        }
                        else
                        {
                            countyColors[(t * vertsPerHex) + 10 + vertOffset] = Color.white;
                            nationColors[(t * vertsPerHex) + 10 + vertOffset] = Color.white;
                        }

                        // Triangle Nine (A - FB - F)
                        mapTris[(t * trisPerHex) + 24 + triOffset] = (t * vertsPerHex) + 0 + vertOffset;
                        mapTris[(t * trisPerHex) + 25 + triOffset] = (t * vertsPerHex) + 10 + vertOffset;
                        mapTris[(t * trisPerHex) + 26 + triOffset] = (t * vertsPerHex) + 8 + vertOffset;

                        // Triangle Ten (A - B - FB)
                        mapTris[(t * trisPerHex) + 27 + triOffset] = (t * vertsPerHex) + 0 + vertOffset;
                        mapTris[(t * trisPerHex) + 28 + triOffset] = (t * vertsPerHex) + 1 + vertOffset;
                        mapTris[(t * trisPerHex) + 29 + triOffset] = (t * vertsPerHex) + 10 + vertOffset;

                        vertOffset = -2;
                        triOffset = -6;
                    }
                }

                vMod = hexes.Length * vertsPerHex;
                tMod = hexes.Length * trisPerHex;

                // Add the wall verts.
                for (int i = 0; i < wallVerts.Length; i++)
                {
                    mapVerts[i + vMod] = wallVerts[i];
                }

                // Add the wall tris.
                for (int i = 0; i < wallTris.Length; i++)
                {
                    wallTris[i] += vMod;
                    mapTris[i + tMod] = wallTris[i];
                }
            }
        }
        #endregion

        #region Simulation Setup
        public void CreateMapTiles(HexSphereGenerator hGen)
        {
            // This just maps hexes to tiles.
            // I split them up to reduce overlap...
            // between world generation and gameplay...
            // so that changes to the latter don't accidentally...
            // step on the toes of the former.

            for (int i = 0; i < hexes.Length; i++)
            {
                Hex h = hexes[i];

                Map.Tile tile = new Map.Tile(h);
                hGen.primitiveTiles.Add(tile);
                h.tile = tile;
            }
        }

        public void SpawnObjects(HexSphereGenerator hGen)
        {
            List<ProceduralGeneration.RockStructure> peaks = new List<ProceduralGeneration.RockStructure>();
            List<ProceduralGeneration.RockStructure> boulders = new List<ProceduralGeneration.RockStructure>();

            List<ProceduralGeneration.Tree> continentalPines = new List<ProceduralGeneration.Tree>();
            List<ProceduralGeneration.Tree> subtropicPines = new List<ProceduralGeneration.Tree>();
            List<ProceduralGeneration.Tree> highlandPines = new List<ProceduralGeneration.Tree>();
            List<ProceduralGeneration.Tree> subarcticPines = new List<ProceduralGeneration.Tree>();

            List<ProceduralGeneration.Tree> tropicalTrees = new List<ProceduralGeneration.Tree>();
            List<ProceduralGeneration.Tree> monsoonTrees = new List<ProceduralGeneration.Tree>();
            List<ProceduralGeneration.Tree> savannaTrees = new List<ProceduralGeneration.Tree>();

            List<ProceduralGeneration.Tree> mediterraneanTrees = new List<ProceduralGeneration.Tree>();
            List<ProceduralGeneration.Tree> prairieTrees = new List<ProceduralGeneration.Tree>();

            for (int i = 0; i < hexes.Length; i++)
            {
                Hex h = hexes[i];

                if (hGen.worldSettings.renderMountains)
                {
                    #region Peaks Gen
                    if (h.tile.fault && !h.pent && !h.tile.shore)
                    {
                        Vector3[] basePoints = new Vector3[h.vertices.Length];

                        for (int v = 0; v < h.vertices.Length; v++)
                        {
                            basePoints[v] = h.vertices[v].pos;
                        }

                        ProceduralGeneration.RockStructure newPeak = new ProceduralGeneration.RockStructure(ProceduralGeneration.RockStructure.RockType.peak, hGen.procSettings, h.center.pos, basePoints, hGen.worldSettings.peakHeight);
                        peaks.Add(newPeak);
                    }
                    #endregion
                }
                if (hGen.worldSettings.renderBoulders && Random.Range(1, 101) > hGen.worldSettings.boulderChance)
                {
                    #region Boulder Gen
                    if (h.tile.biome == Map.Tile.Biome.hotSteppe || h.tile.biome == Map.Tile.Biome.prairie || h.tile.biome == Map.Tile.Biome.savanna)
                    {
                        for (int boulderPoints = 0; boulderPoints < hGen.worldSettings.bouldersPerHex; boulderPoints++)
                        {
                            Vertex r1 = h.vertices[Random.Range(0, h.vertices.Length)];
                            Vertex r2 = h.vertices[Random.Range(0, h.vertices.Length)];

                            if (r2 == r1)
                            {
                                r2 = h.center;
                            }

                            Vector3 boulderCenter = Vector3.Lerp(Vector3.Lerp(r1.pos, r2.pos, Random.Range(0.01f, 0.99f)), h.center.pos, 0.1f);
                            ProceduralGeneration.RockStructure newBoulder = new ProceduralGeneration.RockStructure(ProceduralGeneration.RockStructure.RockType.boulder, hGen.procSettings, boulderCenter);
                            boulders.Add(newBoulder);
                        }
                    }
                    #endregion
                }
                if (hGen.worldSettings.renderTrees)
                {
                    #region Tree Gen
                    if (h.tile.biome == Map.Tile.Biome.humidContinental || h.tile.biome == Map.Tile.Biome.humidSubtropic || h.tile.biome == Map.Tile.Biome.highlands || h.tile.biome == Map.Tile.Biome.subarctic)
                    {
                        int stumpPoints = hGen.worldSettings.pineTreesPerHex;

                        if (h.tile.biome == Map.Tile.Biome.highlands)
                        {
                            stumpPoints = hGen.worldSettings.pineTreesPerHex / 2;
                        }

                        Color canopyColor;

                        if (h.tile.biome == Map.Tile.Biome.humidContinental)
                        {
                            canopyColor = hGen.worldSettings.continentalForestColor;
                        }
                        else if (h.tile.biome == Map.Tile.Biome.humidSubtropic)
                        {
                            canopyColor = hGen.worldSettings.subtropicForestColor;
                        }
                        else if (h.tile.biome == Map.Tile.Biome.subarctic)
                        {
                            canopyColor = hGen.worldSettings.subarcticForestColor;
                        }
                        else
                        {
                            canopyColor = hGen.worldSettings.highlandForestColor;
                        }

                        for (int stumps = 0; stumps < stumpPoints; stumps++)
                        {
                            Vertex r1 = h.vertices[Random.Range(0, h.vertices.Length)];
                            Vertex r2 = h.vertices[Random.Range(0, h.vertices.Length)];

                            if (r2 == r1)
                            {
                                r2 = h.center;
                            }

                            Vector3 stumpCenter = Vector3.Lerp(Vector3.Lerp(r1.pos, r2.pos, Random.Range(0.01f, 0.99f)), h.center.pos, 0.1f);
                            Procedural.ProceduralGeneration.Tree newTree = new Procedural.ProceduralGeneration.Tree(hGen.procSettings, stumpCenter, Procedural.ProceduralGeneration.Tree.TreeType.pine, canopyColor);

                            if (h.tile.biome == Map.Tile.Biome.humidContinental)
                            {
                                continentalPines.Add(newTree);
                            }
                            else if (h.tile.biome == Map.Tile.Biome.humidSubtropic)
                            {
                                subtropicPines.Add(newTree);
                            }
                            else if (h.tile.biome == Map.Tile.Biome.subarctic)
                            {
                                subarcticPines.Add(newTree);
                            }
                            else
                            {
                                highlandPines.Add(newTree);
                            }
                        }
                    }
                    else if (h.tile.biome == Map.Tile.Biome.tropicalMonsoon || h.tile.biome == Map.Tile.Biome.tropicalRainforest || h.tile.biome == Map.Tile.Biome.savanna)
                    {
                        int stumpPoints = hGen.worldSettings.tropicalTreesPerHex;

                        if (h.tile.biome == Map.Tile.Biome.savanna)
                        {
                            stumpPoints = hGen.worldSettings.tropicalTreesPerHex / 5;
                        }

                        Color canopyColor;

                        if (h.tile.biome == Map.Tile.Biome.tropicalMonsoon)
                        {
                            canopyColor = hGen.worldSettings.monsoonForestColor;
                        }
                        else if (h.tile.biome == Map.Tile.Biome.tropicalRainforest)
                        {
                            canopyColor = hGen.worldSettings.tropicalForestColor;
                        }
                        else
                        {
                            canopyColor = hGen.worldSettings.savannaForestColor;
                        }

                        for (int stumps = 0; stumps < stumpPoints; stumps++)
                        {
                            Vertex r1 = h.vertices[Random.Range(0, h.vertices.Length)];
                            Vertex r2 = h.vertices[Random.Range(0, h.vertices.Length)];

                            if (r2 == r1)
                            {
                                r2 = h.center;
                            }

                            Vector3 newStump = Vector3.Lerp(Vector3.Lerp(r1.pos, r2.pos, Random.Range(0.01f, 0.99f)), h.center.pos, 0.1f);
                            Procedural.ProceduralGeneration.Tree newTree = new Procedural.ProceduralGeneration.Tree(hGen.procSettings, newStump, Procedural.ProceduralGeneration.Tree.TreeType.tropical, canopyColor);

                            if (h.tile.biome == Map.Tile.Biome.tropicalMonsoon)
                            {
                                monsoonTrees.Add(newTree);
                            }
                            else if (h.tile.biome == Map.Tile.Biome.tropicalRainforest)
                            {
                                tropicalTrees.Add(newTree);
                            }
                            else
                            {
                                savannaTrees.Add(newTree);
                            }
                        }
                    }
                    else if (h.tile.biome == Map.Tile.Biome.mediterranean && Random.Range(1, 101) > hGen.worldSettings.coastalTreeChance || h.tile.biome == Map.Tile.Biome.prairie && Random.Range(1, 101) > hGen.worldSettings.coastalTreeChance)
                    {
                        int stumpPoints = hGen.worldSettings.coastalTreesPerHex;

                        //if (h.tile.biome == Map.Tile.Biome.prairie)
                        //{
                        //    stumpPoints = hGen.worldSettings.coastalTreesPerHex / 2;
                        //}

                        Color canopyColor;

                        if (h.tile.biome == Map.Tile.Biome.mediterranean)
                        {
                            canopyColor = hGen.worldSettings.mediterraneanForestColor;
                        }
                        else
                        {
                            canopyColor = hGen.worldSettings.prairieForestColor;
                        }

                        for (int stumps = 0; stumps < stumpPoints; stumps++)
                        {
                            Vertex r1 = h.vertices[Random.Range(0, h.vertices.Length)];
                            Vertex r2 = h.vertices[Random.Range(0, h.vertices.Length)];

                            if (r2 == r1)
                            {
                                r2 = h.center;
                            }

                            Vector3 newStump = Vector3.Lerp(Vector3.Lerp(r1.pos, r2.pos, Random.Range(0.01f, 0.99f)), h.center.pos, 0.1f);
                            Procedural.ProceduralGeneration.Tree newTree = new Procedural.ProceduralGeneration.Tree(hGen.procSettings, newStump, Procedural.ProceduralGeneration.Tree.TreeType.coastal, canopyColor);

                            if (h.tile.biome == Map.Tile.Biome.mediterranean)
                            {
                                mediterraneanTrees.Add(newTree);
                            }
                            else if (h.tile.biome == Map.Tile.Biome.prairie)
                            {
                                tropicalTrees.Add(newTree);
                            }
                            else
                            {
                                prairieTrees.Add(newTree);
                            }
                        }
                    }
                    #endregion
                }
            }

            #region Peak Container Gen
            if (peaks.Count > 0)
            {
                GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                g.GetComponent<Map.ProceduralContainer>().MapObjects(peaks);
                g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.mountainColor);
            }
            #endregion

            #region Boulder Container Gen
            if (boulders.Count > 0)
            {
                GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                g.GetComponent<Map.ProceduralContainer>().MapObjects(boulders);
                g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.boulderColor);
            }
            #endregion

            #region Tree Container Gen

            #region Continental Forests
            if (continentalPines.Count > 0 && continentalPines.Count < 1000)
            {
                GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                g.GetComponent<Map.ProceduralContainer>().MapObjects(continentalPines);
                g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.continentalForestColor);
            }
            else
            {
                for (int i = 0; i < continentalPines.Count; i += 999)
                {
                    int takeAmount = Mathf.Min(999, continentalPines.Count - i);

                    GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                    g.GetComponent<Map.ProceduralContainer>().MapObjects(continentalPines.Skip(i).Take(takeAmount).ToList());
                    g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.continentalForestColor);
                }
            }
            #endregion

            #region Subtropic Forests
            if (subtropicPines.Count > 0 && subtropicPines.Count < 1000)
            {
                GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                g.GetComponent<Map.ProceduralContainer>().MapObjects(subtropicPines);
                g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.subtropicForestColor);
            }
            else
            {
                for (int i = 0; i < subtropicPines.Count; i += 999)
                {
                    int takeAmount = Mathf.Min(999, subtropicPines.Count - i);

                    GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                    g.GetComponent<Map.ProceduralContainer>().MapObjects(subtropicPines.Skip(i).Take(takeAmount).ToList());
                    g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.subtropicForestColor);
                }
            }
            #endregion

            #region Subarctic Forests
            if (subarcticPines.Count > 0 && subarcticPines.Count < 1000)
            {
                GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                g.GetComponent<Map.ProceduralContainer>().MapObjects(subarcticPines);
                g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.subarcticForestColor);
            }
            else
            {
                for (int i = 0; i < subarcticPines.Count; i += 999)
                {
                    int takeAmount = Mathf.Min(999, subarcticPines.Count - i);

                    GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                    g.GetComponent<Map.ProceduralContainer>().MapObjects(subarcticPines.Skip(i).Take(takeAmount).ToList());
                    g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.subarcticForestColor);
                }
            }
            #endregion

            #region Highland Forests
            if (highlandPines.Count > 0 && highlandPines.Count < 1000)
            {
                GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                g.GetComponent<Map.ProceduralContainer>().MapObjects(highlandPines);
                g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.highlandForestColor);
            }
            else
            {
                for (int i = 0; i < highlandPines.Count; i += 999)
                {
                    int takeAmount = Mathf.Min(999, highlandPines.Count - i);

                    GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                    g.GetComponent<Map.ProceduralContainer>().MapObjects(highlandPines.Skip(i).Take(takeAmount).ToList());
                    g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.highlandForestColor);
                }
            }
            #endregion

            #region Monsoon Forests
            if (monsoonTrees.Count > 0 && monsoonTrees.Count < 1000)
            {
                GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                g.GetComponent<Map.ProceduralContainer>().MapObjects(monsoonTrees);
                g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.monsoonForestColor);
            }
            else
            {
                for (int i = 0; i < monsoonTrees.Count; i += 999)
                {
                    int takeAmount = Mathf.Min(999, monsoonTrees.Count - i);

                    GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                    g.GetComponent<Map.ProceduralContainer>().MapObjects(monsoonTrees.Skip(i).Take(takeAmount).ToList());
                    g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.monsoonForestColor);
                }
            }
            #endregion

            #region Tropical Rainforests
            if (tropicalTrees.Count > 0 && tropicalTrees.Count < 1000)
            {
                GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                g.GetComponent<Map.ProceduralContainer>().MapObjects(tropicalTrees);
                g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.tropicalForestColor);
            }
            else
            {
                for (int i = 0; i < tropicalTrees.Count; i += 999)
                {
                    int takeAmount = Mathf.Min(999, tropicalTrees.Count - i);

                    GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                    g.GetComponent<Map.ProceduralContainer>().MapObjects(tropicalTrees.Skip(i).Take(takeAmount).ToList());
                    g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.tropicalForestColor);
                }
            }
            #endregion

            #region Savanna Forests
            if (savannaTrees.Count > 0 && savannaTrees.Count < 1000)
            {
                GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                g.GetComponent<Map.ProceduralContainer>().MapObjects(savannaTrees);
                g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.savannaForestColor);
            }
            else
            {
                for (int i = 0; i < savannaTrees.Count; i += 999)
                {
                    int takeAmount = Mathf.Min(999, savannaTrees.Count - i);

                    GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                    g.GetComponent<Map.ProceduralContainer>().MapObjects(savannaTrees.Skip(i).Take(takeAmount).ToList());
                    g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.savannaForestColor);
                }
            }
            #endregion

            #region Mediterranean Forests
            if (mediterraneanTrees.Count > 0 && mediterraneanTrees.Count < 1000)
            {
                GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                g.GetComponent<Map.ProceduralContainer>().MapObjects(mediterraneanTrees);
                g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.mediterraneanForestColor);
            }
            else
            {
                for (int i = 0; i < mediterraneanTrees.Count; i += 999)
                {
                    int takeAmount = Mathf.Min(999, mediterraneanTrees.Count - i);

                    GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                    g.GetComponent<Map.ProceduralContainer>().MapObjects(mediterraneanTrees.Skip(i).Take(takeAmount).ToList());
                    g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.mediterraneanForestColor);
                }
            }
            #endregion

            #region Prairie Forests
            if (prairieTrees.Count > 0 && prairieTrees.Count < 1000)
            {
                GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                g.GetComponent<Map.ProceduralContainer>().MapObjects(prairieTrees);
                g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.prairieForestColor);
            }
            else
            {
                for (int i = 0; i < prairieTrees.Count; i += 999)
                {
                    int takeAmount = Mathf.Min(999, prairieTrees.Count - i);

                    GameObject g = Instantiate(proceduralPrefab, this.transform.position, Quaternion.identity, this.transform);
                    g.GetComponent<Map.ProceduralContainer>().MapObjects(prairieTrees.Skip(i).Take(takeAmount).ToList());
                    g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.prairieForestColor);
                }
            }
            #endregion
            #endregion
        }

        public void AddBuilding(ProceduralGeneration.Building newBuilding)
        {
            // Structures are going to be rendered per chunk on a "cluster" basis.
            // That means that rather than having each structure be its own object,
            // which could get slow once we have lots of structures, we'll have...
            // nearby structures get rendered together.
            // Let's just say 100 structures per structure chunk.

            GameObject viableCluster = null;

            foreach (GameObject g in structureClusters)
            {
                if (g.GetComponent<Map.StructureCluster>().triCount + (newBuilding.triangles.Length / 3) < 1000)
                {
                    viableCluster = g;
                    break;
                }
            }

            if (viableCluster == null)
            {
                GameObject newCluster = Instantiate(structureClusterPrefab, this.transform.position, Quaternion.identity, this.transform);
                structureClusters.Add(newCluster);
                viableCluster = newCluster;
            }

            viableCluster.GetComponent<Map.StructureCluster>().AddBuilding(newBuilding);
        }

        public void SpawnGrass(HexSphereGenerator hGen)
        {
            // We want to create mesh objects to hold the grass terrain...
            // on the specific hexes that have grass, and we want all instances...
            // of a particular kind (read: color) of grass to be under their own...
            // mesh object.

            Dictionary<Map.Tile.Biome, GameObject> bladeDictionary = new Dictionary<Map.Tile.Biome, GameObject>();

            for (int i = 0; i < hexes.Length; i++)
            {
                Hex h = hexes[i];
                Map.Tile t = h.tile;

                if (t.grass && !bladeDictionary.ContainsKey(t.biome))
                {
                    GameObject g = Instantiate(grassPrefab, this.transform.position, Quaternion.identity, this.transform);

                    if (t.biome == Map.Tile.Biome.savanna)
                    {
                        g.GetComponent<GrassController>().baseColor = hGen.worldSettings.savannahGrassBaseColor;
                        g.GetComponent<GrassController>().tipColor = hGen.worldSettings.savannahGrassTipColor;
                    }
                    else if (t.biome == Map.Tile.Biome.hotSteppe)
                    {
                        g.GetComponent<GrassController>().baseColor = hGen.worldSettings.steppeGrassBaseColor;
                        g.GetComponent<GrassController>().tipColor = hGen.worldSettings.steppeGrassTipColor;
                    }
                    else if (t.biome == Map.Tile.Biome.mediterranean)
                    {
                        g.GetComponent<GrassController>().baseColor = hGen.worldSettings.mediterraneanGrassBaseColor;
                        g.GetComponent<GrassController>().tipColor = hGen.worldSettings.mediterraneanGrassTipColor;
                    }
                    else if (t.biome == Map.Tile.Biome.oceanic)
                    {
                        g.GetComponent<GrassController>().baseColor = hGen.worldSettings.oceanicGrassBaseColor;
                        g.GetComponent<GrassController>().tipColor = hGen.worldSettings.oceanicGrassTipColor;
                    }
                    else if (t.biome == Map.Tile.Biome.prairie)
                    {
                        g.GetComponent<GrassController>().baseColor = hGen.worldSettings.prairieGrassBaseColor;
                        g.GetComponent<GrassController>().tipColor = hGen.worldSettings.prairieGrassTipColor;
                    }
                    else if (t.biome == Map.Tile.Biome.highlands)
                    {
                        g.GetComponent<GrassController>().baseColor = hGen.worldSettings.highlandGrassBaseColor;
                        g.GetComponent<GrassController>().tipColor = hGen.worldSettings.highlandGrassTipColor;
                    }

                    bladeDictionary.Add(t.biome, g);
                    g.GetComponent<GrassController>().hexes.Add(h);
                }
                else if (t.grass)
                {
                    bladeDictionary[t.biome].GetComponent<GrassController>().hexes.Add(h);
                }
            }

            foreach (KeyValuePair<Map.Tile.Biome, GameObject> bG in bladeDictionary)
            {
                bG.Value.GetComponent<GrassController>().Render();
            }
        }

        #endregion

        #region Hex Generation & Location
        public void Hexify(HexSphereGenerator hGen)
        {
            // Converts triangles into hexes.
            int mod = 0;

            if (number == 0 || number == 19)
            {
                mod = 1;
            }

            hexCenters = new Dictionary<Vector3, Hex>();
            hexes = new Hex[CountUniqueVerts(hGen.vertHexes) + mod];
            int hFound = 0;

            for (int i = 0; i < triangles.Length; i++)
            {
                Vertex v = triangles[i].vC;

                if (!hGen.vertHexes.ContainsKey(v.pos))
                {
                    // Set up new hex.
                    Hex newHex = new Hex
                    {
                        center = new Vertex(v.pos),
                        color = color
                    };
                    hGen.vertHexes.Add(v.pos, newHex);
                    newHex.chunk = this;

                    // Add vertices.
                    if (origin.vA.pos == v.pos || origin.vB.pos == v.pos || origin.vC.pos == v.pos)
                    {
                        // Adds a pentagon to an "original" vertex.

                        newHex.pent = true;
                        newHex.vertices = new Vertex[5];

                        Vertex[] vertsFound = FindVertices(hGen, v).ToArray();

                        newHex.vertices[0] = vertsFound[0];
                        newHex.vertices[1] = vertsFound[1];
                        newHex.vertices[2] = vertsFound[2];
                        newHex.vertices[3] = vertsFound[3];
                        newHex.vertices[4] = vertsFound[4];

                        newHex.CalculateCenter();
                    }
                    else
                    {
                        // Adds a hexagon over a vertex.

                        newHex.vertices = new Vertex[6];

                        Vertex[] vertsFound = FindVertices(hGen, v).ToArray();

                        newHex.vertices[0] = vertsFound[0];
                        newHex.vertices[1] = vertsFound[1];
                        newHex.vertices[2] = vertsFound[2];
                        newHex.vertices[3] = vertsFound[3];
                        newHex.vertices[4] = vertsFound[4];
                        newHex.vertices[5] = vertsFound[5];

                        newHex.CalculateCenter();
                    }

                    FindHexNeighbors(hGen, newHex, v);
                    hexCenters.Add(newHex.center.pos, newHex);
                    hexes[hFound] = newHex;
                    hGen.unsortedHexes.Add(newHex);
                    hFound++;
                }
            }

            hGen.maxContDist = (hexes[0].vertices[0].pos - hexes[0].vertices[1].pos).sqrMagnitude;

            // Now we need to add the pole caps.
            // We'll just add these to the first and last chunks.

            if (number == 0)
            {
                Vertex v = new Vertex(new Vector3(0, hGen.worldRadius, 0));

                Hex newHex = new Hex
                {
                    center = new Vertex(v.pos)
                };
                hGen.vertHexes.Add(v.pos, newHex);
                newHex.color = color;
                newHex.vertices = new Vertex[6];

                newHex.pent = true;
                newHex.vertices = new Vertex[5];

                Vertex[] vertsFound = FindVertices(hGen, v).ToArray();

                newHex.vertices[0] = vertsFound[0];
                newHex.vertices[1] = vertsFound[1];
                newHex.vertices[2] = vertsFound[2];
                newHex.vertices[3] = vertsFound[3];
                newHex.vertices[4] = vertsFound[4];

                newHex.CalculateCenter();
                FindHexNeighbors(hGen, newHex, v);

                hexCenters.Add(newHex.center.pos, newHex);
                hexes[hFound] = newHex;
                hGen.unsortedHexes.Add(newHex);
            }
            else if (number == 19)
            {
                Vertex v = new Vertex(new Vector3(0, -hGen.worldRadius, 0));

                Hex newHex = new Hex
                {
                    center = new Vertex(v.pos)
                };
                hGen.vertHexes.Add(v.pos, newHex);
                newHex.color = color;
                newHex.vertices = new Vertex[6];

                newHex.pent = true;
                newHex.vertices = new Vertex[5];

                Vertex[] vertsFound = FindVertices(hGen, v).ToArray();

                newHex.vertices[0] = vertsFound[0];
                newHex.vertices[1] = vertsFound[1];
                newHex.vertices[2] = vertsFound[2];
                newHex.vertices[3] = vertsFound[3];
                newHex.vertices[4] = vertsFound[4];

                newHex.CalculateCenter();
                FindHexNeighbors(hGen, newHex, v);

                hexCenters.Add(newHex.center.pos, newHex);
                hexes[hFound] = newHex;
                hGen.unsortedHexes.Add(newHex);
            }
        }

        public Hex ListNearestHex(Vector3 worldSpace)
        {
            Hex closestHex = null;
            float closestDistance = Mathf.Infinity;

            for (int i = 0; i < hexes.Length; i++)
            {
                Hex h = hexes[i];

                if ((h.center.pos - worldSpace).sqrMagnitude < closestDistance)
                {
                    closestDistance = (h.center.pos - worldSpace).sqrMagnitude;
                    closestHex = h;
                }
            }

            return closestHex;
        }

        public void AddHexNoise(NoiseFilter noiseFilter, NoiseSettings noiseSettings, HexSphereGenerator hGen)
        {
            List<Vector3> wallVertsList = new List<Vector3>();
            List<int> wallTrisList = new List<int>();

            for (int x = 0; x < hexes.Length; x++)
            {
                Hex hex = hexes[x];

                float elevation = noiseFilter.Evaluate(hex.center.pos);
                Vector3 normal = (hex.center.pos - hGen.gameObject.transform.position).normalized;

                int intElev = Mathf.RoundToInt(elevation);

                if (hex.tile.fault)
                {
                    float weathering = 1;

                    if (hex.tile.shore)
                    {
                        weathering = noiseSettings.mountainWeathering;
                    }

                    intElev = Mathf.RoundToInt(Mathf.Abs(intElev + (intElev * (noiseSettings.mountainBuff * weathering))));
                }
                else if (hex.tile.faultAdjacent && !hex.tile.shore)
                {
                    float weathering = 1;

                    // I'm thinking we'll want this to not actually affect the coast...
                    // if we want people to not be able to pass over mountains.

                    //if (hex.tile.shore)
                    //{
                    //    weathering = noiseSettings.mountainWeathering;
                    //}

                    intElev = Mathf.RoundToInt(Mathf.Abs(intElev + (intElev * (noiseSettings.mountainSloping * weathering))));
                }

                if (hex.tile.polarCap)
                {
                    float oceanOffset = noiseSettings.polarCapMin * (hGen.oceanRadius - hex.center.pos.magnitude);

                    if (oceanOffset < 0)
                    {
                        oceanOffset = 0;
                    }

                    intElev = Mathf.RoundToInt(oceanOffset + Mathf.Abs(intElev / noiseSettings.polarCapDebuff));
                }

                if (intElev % noiseSettings.terraceCutoff != 0)
                {
                    intElev += noiseSettings.terracing;
                }

                hex.center.pos += normal * intElev;

                //if (hex.tile.fault)
                //{
                //    hex.center.pos += hex.center.pos.normalized * Random.Range(hGen.worldSettings.peakHeight, hGen.worldSettings.peakHeight * 2);
                //}

                for (int i = 0; i < hex.vertices.Length; i++)
                {
                    Vertex v = hex.vertices[i];

                    if (i != hex.vertices.Length - 1)
                    {
                        wallVertsList.Add(v.pos);

                        wallTrisList.Add(wallVertsList.Count - 1);
                        wallTrisList.Add(wallVertsList.Count + 0);
                        wallTrisList.Add(wallVertsList.Count + 1);

                        wallTrisList.Add(wallVertsList.Count + 1);
                        wallTrisList.Add(wallVertsList.Count + 0);
                        wallTrisList.Add(wallVertsList.Count + 2);
                    }
                    else
                    {
                        int pentMod = 0;

                        if (hex.pent)
                        {
                            pentMod = 2;
                        }

                        wallVertsList.Add(v.pos);

                        wallTrisList.Add(wallVertsList.Count - 1);
                        wallTrisList.Add(wallVertsList.Count + 0);
                        wallTrisList.Add(wallVertsList.Count - 11 + pentMod);

                        wallTrisList.Add(wallVertsList.Count - 11 + pentMod);
                        wallTrisList.Add(wallVertsList.Count + 0);
                        wallTrisList.Add(wallVertsList.Count - 10 + pentMod);
                    }

                    normal = (v.pos - hGen.gameObject.transform.position).normalized;
                    v.pos += normal * intElev;

                    wallVertsList.Add(v.pos);
                }
            }

            wallVerts = wallVertsList.ToArray();
            wallTris = wallTrisList.ToArray();
        }

        public void CullHexes()
        {
            for (int i = 0; i < hexes.Length; i++)
            {
                Hex h = hexes[i];

                h.neighbors = (from Hex n in h.neighbors where n.tile != null select n).ToList();
            }
        }
        #endregion

        #region Neighbors & Sorting
        public void FindHexNeighbors(HexSphereGenerator hGen, Hex h, Vertex oldCenter)
        {
            List<Vector3> vertNeighbors = FindTriangleCorners(hGen, oldCenter);

            foreach (Vector3 v in vertNeighbors)
            {
                if (hGen.vertHexes.ContainsKey(v))
                {
                    if (hGen.vertHexes[v] != h)
                    {
                        h.AddNeighbor(hGen.vertHexes[v]);
                    }
                }
            }
        }

        public Vertex[] FindVertices(HexSphereGenerator hGen, Vertex p)
        {
            // Finds all the neighboring vertices where a hex's corners will go.

            List<Triangle> triNeighbors = hGen.vecTriangleNeighbors[p.pos];
            Vertex[] vertNeighbors = new Vertex[triNeighbors.Count];

            for (int t = 0; t < triNeighbors.Count; t++)
            {
                Triangle tri = triNeighbors[t];

                vertNeighbors[t] = new Vertex((tri.vA.pos + tri.vB.pos + tri.vC.pos) / 3.0f);
            }

            vertNeighbors = SortNeighbors(vertNeighbors, p);

            return vertNeighbors;
        }

        public List<Vector3> FindTriangleCorners(HexSphereGenerator hGen, Vertex p)
        {
            // Finds all the neighboring vertices where a hex's corners will go.

            List<Triangle> triNeighbors = hGen.vecTriangleNeighbors[p.pos];
            List<Vector3> vertNeighbors = new List<Vector3>();

            for (int t = 0; t < triNeighbors.Count; t++)
            {
                Triangle tri = triNeighbors[t];

                vertNeighbors.Add(tri.vA.pos);
                vertNeighbors.Add(tri.vB.pos);
                vertNeighbors.Add(tri.vC.pos);
            }

            return vertNeighbors;
        }

        public Vertex[] SortNeighbors(Vertex[] points, Vertex center)
        {
            // Sorts the vertices from "FindVertices" so that they are...
            // clockwise relative to the exterior of the "sphere."

            Vertex[] sortedNeighbors = new Vertex[6];
            Vertex selectNeighbor = points.ToList()[Random.Range(0, points.Length)];
            List<Vertex> checkedNeighbors = new List<Vertex>();

            int run = 0;

            while (run < points.Length)
            {
                sortedNeighbors[run] = selectNeighbor;
                checkedNeighbors.Add(selectNeighbor);

                Vertex closestNeighbor = null;
                float closestDistance = Mathf.Infinity;

                if (run == 0)
                {
                    List<Vertex> closestNeighbors = new List<Vertex>();
                    closestNeighbors = points.OrderBy((q) => (q.pos - selectNeighbor.pos).sqrMagnitude).ToList();
                    closestNeighbors.Remove(selectNeighbor);

                    Vertex c1 = closestNeighbors[0];
                    Vertex c2 = closestNeighbors[1];


                    Vector3 crossN = Vector3.Cross((c2.pos - selectNeighbor.pos), (c1.pos - selectNeighbor.pos));
                    float w = Vector3.Dot(crossN, (selectNeighbor.pos - center.pos));

                    if (w > 0)
                    {
                        closestNeighbor = c2;
                    }
                    else
                    {
                        closestNeighbor = c1;
                    }
                }
                else
                {
                    foreach (Vertex s in points)
                    {
                        if (!checkedNeighbors.Contains(s))
                        {
                            if ((s.pos - selectNeighbor.pos).sqrMagnitude < closestDistance)
                            {
                                closestDistance = (s.pos - selectNeighbor.pos).sqrMagnitude;
                                closestNeighbor = s;
                            }
                        }
                    }
                }

                run++;
                selectNeighbor = closestNeighbor;
            }

            return sortedNeighbors;

        }

        public void SortHexNeighbors()
        {
            for (int hi = 0; hi < hexes.Length; hi++)
            {
                Hex center = hexes[hi];

                // Sorts a hex's neighbors from the top, clockwise.
                Hex[] sortedNeighbors = new Hex[6];

                if (center.pent)
                {
                    sortedNeighbors = new Hex[5];
                }

                Vector3[] axes = FindRelativeAxes(center.center);

                Hex selectNeighbor = center.neighbors.OrderBy((p) => ((((Vector3.up * 3) + axes[2]) + center.center.pos) - p.center.pos).sqrMagnitude).ToList()[0];

                List<Hex> checkedNeighbors = new List<Hex>();

                int run = 0;

                while (run < center.neighbors.Count)
                {
                    sortedNeighbors[run] = selectNeighbor;
                    checkedNeighbors.Add(selectNeighbor);

                    Hex closestNeighbor = null;
                    float closestDistance = Mathf.Infinity;

                    if (run == 0)
                    {
                        List<Hex> closestNeighbors = new List<Hex>();
                        closestNeighbors = center.neighbors.OrderBy((q) => (q.center.pos - selectNeighbor.center.pos).sqrMagnitude).ToList();
                        closestNeighbors.Remove(selectNeighbor);

                        Hex c1 = closestNeighbors[0];
                        Hex c2 = closestNeighbors[1];

                        Vector3 crossN = Vector3.Cross((c2.center.pos - selectNeighbor.center.pos), (c1.center.pos - selectNeighbor.center.pos));
                        float w = Vector3.Dot(crossN, (selectNeighbor.center.pos - center.center.pos));

                        if (w > 0)
                        {
                            closestNeighbor = c2;
                        }
                        else
                        {
                            closestNeighbor = c1;
                        }
                    }
                    else
                    {
                        foreach (Hex s in center.neighbors)
                        {
                            if (!checkedNeighbors.Contains(s))
                            {
                                if ((s.center.pos - selectNeighbor.center.pos).sqrMagnitude < closestDistance)
                                {
                                    closestDistance = (s.center.pos - selectNeighbor.center.pos).sqrMagnitude;
                                    closestNeighbor = s;
                                }
                            }
                        }
                    }

                    run++;
                    selectNeighbor = closestNeighbor;
                }

                center.neighbors = sortedNeighbors.ToList();
            }
        }

        #endregion

        #region Counting & Subdivision
        int CountUniqueVerts(Dictionary<Vector3, Hex> checkedVerts)
        {
            // Determines how many vertices, and thus hexes, the chunk will have.

            List<Vector3> uniqueVerts = new List<Vector3>();

            for (int i = 0; i < triangles.Length; i++)
            {
                Triangle t = triangles[i];

                if (!uniqueVerts.Contains(t.vC.pos) && !checkedVerts.ContainsKey(t.vC.pos))
                {
                    uniqueVerts.Add(t.vC.pos);
                }
            }

            return uniqueVerts.Count;
        }

        public void Subdivide(float radius)
        {
            // Takes each triangle and splits it into four.

            vertices = new Vertex[triangles.Length * 6];
            Triangle[] newTriangles = new Triangle[triangles.Length * 4];

            for (int i = 0; i < triangles.Length; i++)
            {
                Triangle oldTri = triangles[i];

                int flipMod = 1;

                if (oldTri.vB.pos.y < (oldTri.vA.pos.y + oldTri.vC.pos.y) / 2.0f)
                {
                    flipMod = -1;
                }

                Vertex a = new Vertex(oldTri.vA.pos)
                {
                    x = oldTri.vA.x,
                    y = oldTri.vA.y
                };

                Vertex ab = new Vertex((oldTri.vA.pos + oldTri.vB.pos) / 2.0f)
                {
                    x = oldTri.vB.x,
                    y = oldTri.vB.y
                };

                Vertex b = new Vertex(oldTri.vB.pos)
                {
                    x = oldTri.vB.x + 1,
                    y = oldTri.vB.y + flipMod
                };

                Vertex bc = new Vertex((oldTri.vB.pos + oldTri.vC.pos) / 2.0f)
                {
                    x = oldTri.vC.x,
                    y = oldTri.vC.y + flipMod
                };

                Vertex c = new Vertex(oldTri.vC.pos)
                {
                    x = oldTri.vC.x + 1,
                    y = oldTri.vC.y
                };

                Vertex ca = new Vertex((oldTri.vC.pos + oldTri.vA.pos) / 2.0f)
                {
                    x = oldTri.vC.x,
                    y = oldTri.vC.y
                };


                Triangle aabca = new Triangle(a, ab, ca);
                aabca.NormalizeVertices(radius);
                newTriangles[i * 4 + 0] = aabca;

                Triangle abbbc = new Triangle(ab, b, bc);
                abbbc.NormalizeVertices(radius);
                newTriangles[i * 4 + 1] = abbbc;

                Triangle cabcc = new Triangle(ca, bc, c);
                cabcc.NormalizeVertices(radius);
                newTriangles[i * 4 + 2] = cabcc;

                Triangle abbcca = new Triangle(ab, bc, ca);
                abbcca.NormalizeVertices(radius);
                newTriangles[i * 4 + 3] = abbcca;

                vertices[(i * 6) + 0] = a;
                vertices[(i * 6) + 1] = ab;
                vertices[(i * 6) + 2] = b;
                vertices[(i * 6) + 3] = bc;
                vertices[(i * 6) + 4] = c;
                vertices[(i * 6) + 5] = ca;
            }

            origin.NormalizeVertices(radius);

            triangles = newTriangles;
        }
        #endregion

        #region Misc

        public int[] FindNeighbors(int o)
        {
            // There must be a more elegent way to do this...
            // but I don't want to spend time finding it.

            int r = o + 1;
            int l = o - 1;
            int v;

            // Right Irregularities
            if ((o + 1) % 5 == 0 && o != 9)
            {
                r = (o + 1) / 3;

                if (o == 4)
                {
                    r = 0;
                }
            }

            // Left Irregularities
            if (o == 0)
            {
                l = 4;
            }
            else if (o == 5)
            {
                l = 14;
            }
            else if (o == 15)
            {
                l = 19;
            }

            // Vertical Irregularities
            if (o <= 4)
            {
                v = 5 + (o * 2);
            }
            else if (o <= 14)
            {
                if (o % 2 == 0)
                {
                    v = 15 + ((o - 6) / 2);
                }
                else
                {
                    v = 0 + ((o - 5) / 2);
                }
            }
            else
            {
                v = o - 1;

                if (o == 15)
                {
                    v = 19;
                }
            }

            return new int[] { r, l, v };
        }

        public static Vector3[] FindRelativeAxes(Vertex v)
        {
            Vector3 forward = v.pos.normalized;

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

        public static Vector3 RelativeMovement(Vector3 p, Vector3 forward, Vector3 up, Vector3 right)
        {
            return new Vector3((p.x * right.x) + (p.y * up.x) + (p.z * forward.x), (p.x * right.y) + (p.y * up.y) + (p.z * forward.y), (p.x * right.z) + (p.y * up.z) + (p.z * forward.z));
        }

        public static Vector2 PerpendicularLine(Vector2 vector2)
        {
            return new Vector2(vector2.y, -vector2.x);
        }
        #endregion
    }
}
