using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DeadReckoning.Procedural;


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

        public GameObject grassPrefab;
        public GameObject forestPrefab;

        #region Hidden Variables
        [HideInInspector]
        public Vector3[] mapVerts;

        [HideInInspector]
        public int[] mapTris;

        [HideInInspector]
        public Vector2[] mapUV;
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
                // mesh.uv = mapUV;
                // mesh.colors = mapColors;
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
                foreach (GrassController g in this.gameObject.GetComponentsInChildren<GrassController>(true))
                {
                    if (g.gameObject != this)
                    {
                        g.gameObject.SetActive(true);
                    }
                }
            }
            else
            {
                foreach (GrassController g in this.gameObject.GetComponentsInChildren<GrassController>(true))
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
                mesh.colors = mapColors;
            }
            else if (lens == GlobeCamera.Lens.matte)
            {
                mesh.uv = dummyUV;
                mesh.colors = mapColors;
            }
            else if (lens == GlobeCamera.Lens.winds)
            {
                mesh.uv = dummyUV;
                mesh.colors = windColors;
            }
            else if (lens == GlobeCamera.Lens.currents)
            {
                mesh.uv = dummyUV;
                mesh.colors = currentColors;
            }
            else if (lens == GlobeCamera.Lens.biomes)
            {
                mesh.uv = dummyUV;
                mesh.colors = biomeColors;
            }
            else if (lens == GlobeCamera.Lens.temperature)
            {
                mesh.uv = dummyUV;
                mesh.colors = temperatureColors;
            }
            else if (lens == GlobeCamera.Lens.precipitation)
            {
                mesh.uv = dummyUV;
                mesh.colors = precipitationColors;
            }
            else if (lens == GlobeCamera.Lens.plates)
            {
                mesh.uv = dummyUV;
                mesh.colors = plateColors;
            }

            // mesh.Optimize();
            mesh.RecalculateNormals();
            // mesh.RecalculateBounds();
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

                mapVerts = new Vector3[(hexes.Length * (12 + 1)) + vMod];
                mapTris = new int[(hexes.Length * (12 * 3)) + tMod];
                mapUV = new Vector2[mapVerts.Length];
                dummyUV = new Vector2[mapVerts.Length];
                mapColors = new Color[mapVerts.Length];
                windColors = new Color[mapVerts.Length];
                currentColors = new Color[mapVerts.Length];
                biomeColors = new Color[mapVerts.Length];
                temperatureColors = new Color[mapVerts.Length];
                precipitationColors = new Color[mapVerts.Length];
                plateColors = new Color[mapVerts.Length];

                for (int dum = 0; dum < dummyUV.Length; dum++) // This is used when we want to show colors rather than textures.
                {
                    dummyUV[0] = new Vector2(0, 0);
                }

                int vertOffset = 0; // Used to make sure there aren't gaps in the tri array due to pentagons.
                int triOffset = 0;

                for (int t = 0; t < hexes.Length; t++)
                {
                    Hex h = hexes[t];

                    mapVerts[(t * 13) + 0 + vertOffset] = h.center.pos;                                      // A
                    mapVerts[(t * 13) + 1 + vertOffset] = h.vertices[0].pos;                                // B
                    mapVerts[(t * 13) + 2 + vertOffset] = h.vertices[1].pos;                                // C
                    mapVerts[(t * 13) + 3 + vertOffset] = (h.vertices[0].pos + h.vertices[1].pos) / 2.0f;  // BC
                    mapVerts[(t * 13) + 4 + vertOffset] = h.vertices[2].pos;                                // D
                    mapVerts[(t * 13) + 5 + vertOffset] = (h.vertices[1].pos + h.vertices[2].pos) / 2.0f;  // CD
                    mapVerts[(t * 13) + 6 + vertOffset] = h.vertices[3].pos;                                // E
                    mapVerts[(t * 13) + 7 + vertOffset] = (h.vertices[2].pos + h.vertices[3].pos) / 2.0f;  // DE
                    mapVerts[(t * 13) + 8 + vertOffset] = h.vertices[4].pos;                                // F
                    mapVerts[(t * 13) + 9 + vertOffset] = (h.vertices[3].pos + h.vertices[4].pos) / 2.0f;  // EF

                    mapUV[(t * 13) + 0 + vertOffset] = h.uv;
                    mapUV[(t * 13) + 1 + vertOffset] = h.uv;
                    mapUV[(t * 13) + 2 + vertOffset] = h.uv;
                    mapUV[(t * 13) + 3 + vertOffset] = h.uv;
                    mapUV[(t * 13) + 4 + vertOffset] = h.uv;
                    mapUV[(t * 13) + 5 + vertOffset] = h.uv;
                    mapUV[(t * 13) + 6 + vertOffset] = h.uv;
                    mapUV[(t * 13) + 7 + vertOffset] = h.uv;
                    mapUV[(t * 13) + 8 + vertOffset] = h.uv;
                    mapUV[(t * 13) + 9 + vertOffset] = h.uv;

                    mapColors[(t * 13) + 0 + vertOffset] = h.color;
                    mapColors[(t * 13) + 1 + vertOffset] = h.color;
                    mapColors[(t * 13) + 2 + vertOffset] = h.color;
                    mapColors[(t * 13) + 3 + vertOffset] = h.color;
                    mapColors[(t * 13) + 4 + vertOffset] = h.color;
                    mapColors[(t * 13) + 5 + vertOffset] = h.color;
                    mapColors[(t * 13) + 6 + vertOffset] = h.color;
                    mapColors[(t * 13) + 7 + vertOffset] = h.color;
                    mapColors[(t * 13) + 8 + vertOffset] = h.color;
                    mapColors[(t * 13) + 9 + vertOffset] = h.color;

                    windColors[(t * 13) + 0 + vertOffset] = h.windColor;
                    windColors[(t * 13) + 1 + vertOffset] = h.windColor;
                    windColors[(t * 13) + 2 + vertOffset] = h.windColor;
                    windColors[(t * 13) + 3 + vertOffset] = h.windColor;
                    windColors[(t * 13) + 4 + vertOffset] = h.windColor;
                    windColors[(t * 13) + 5 + vertOffset] = h.windColor;
                    windColors[(t * 13) + 6 + vertOffset] = h.windColor;
                    windColors[(t * 13) + 7 + vertOffset] = h.windColor;
                    windColors[(t * 13) + 8 + vertOffset] = h.windColor;
                    windColors[(t * 13) + 9 + vertOffset] = h.windColor;

                    currentColors[(t * 13) + 0 + vertOffset] = h.currentColor;
                    currentColors[(t * 13) + 1 + vertOffset] = h.currentColor;
                    currentColors[(t * 13) + 2 + vertOffset] = h.currentColor;
                    currentColors[(t * 13) + 3 + vertOffset] = h.currentColor;
                    currentColors[(t * 13) + 4 + vertOffset] = h.currentColor;
                    currentColors[(t * 13) + 5 + vertOffset] = h.currentColor;
                    currentColors[(t * 13) + 6 + vertOffset] = h.currentColor;
                    currentColors[(t * 13) + 7 + vertOffset] = h.currentColor;
                    currentColors[(t * 13) + 8 + vertOffset] = h.currentColor;
                    currentColors[(t * 13) + 9 + vertOffset] = h.currentColor;

                    biomeColors[(t * 13) + 0 + vertOffset] = h.biomeColor;
                    biomeColors[(t * 13) + 1 + vertOffset] = h.biomeColor;
                    biomeColors[(t * 13) + 2 + vertOffset] = h.biomeColor;
                    biomeColors[(t * 13) + 3 + vertOffset] = h.biomeColor;
                    biomeColors[(t * 13) + 4 + vertOffset] = h.biomeColor;
                    biomeColors[(t * 13) + 5 + vertOffset] = h.biomeColor;
                    biomeColors[(t * 13) + 6 + vertOffset] = h.biomeColor;
                    biomeColors[(t * 13) + 7 + vertOffset] = h.biomeColor;
                    biomeColors[(t * 13) + 8 + vertOffset] = h.biomeColor;
                    biomeColors[(t * 13) + 9 + vertOffset] = h.biomeColor;

                    precipitationColors[(t * 13) + 0 + vertOffset] = h.precipitationColor;
                    precipitationColors[(t * 13) + 1 + vertOffset] = h.precipitationColor;
                    precipitationColors[(t * 13) + 2 + vertOffset] = h.precipitationColor;
                    precipitationColors[(t * 13) + 3 + vertOffset] = h.precipitationColor;
                    precipitationColors[(t * 13) + 4 + vertOffset] = h.precipitationColor;
                    precipitationColors[(t * 13) + 5 + vertOffset] = h.precipitationColor;
                    precipitationColors[(t * 13) + 6 + vertOffset] = h.precipitationColor;
                    precipitationColors[(t * 13) + 7 + vertOffset] = h.precipitationColor;
                    precipitationColors[(t * 13) + 8 + vertOffset] = h.precipitationColor;
                    precipitationColors[(t * 13) + 9 + vertOffset] = h.precipitationColor;

                    temperatureColors[(t * 13) + 0 + vertOffset] = h.temperatureColor;
                    temperatureColors[(t * 13) + 1 + vertOffset] = h.temperatureColor;
                    temperatureColors[(t * 13) + 2 + vertOffset] = h.temperatureColor;
                    temperatureColors[(t * 13) + 3 + vertOffset] = h.temperatureColor;
                    temperatureColors[(t * 13) + 4 + vertOffset] = h.temperatureColor;
                    temperatureColors[(t * 13) + 5 + vertOffset] = h.temperatureColor;
                    temperatureColors[(t * 13) + 6 + vertOffset] = h.temperatureColor;
                    temperatureColors[(t * 13) + 7 + vertOffset] = h.temperatureColor;
                    temperatureColors[(t * 13) + 8 + vertOffset] = h.temperatureColor;
                    temperatureColors[(t * 13) + 9 + vertOffset] = h.temperatureColor;

                    if (h.tile.plate != null)
                    {
                        plateColors[(t * 13) + 0 + vertOffset] = h.tile.plate.color;
                        plateColors[(t * 13) + 1 + vertOffset] = h.tile.plate.color;
                        plateColors[(t * 13) + 2 + vertOffset] = h.tile.plate.color;
                        plateColors[(t * 13) + 3 + vertOffset] = h.tile.plate.color;
                        plateColors[(t * 13) + 4 + vertOffset] = h.tile.plate.color;
                        plateColors[(t * 13) + 5 + vertOffset] = h.tile.plate.color;
                        plateColors[(t * 13) + 6 + vertOffset] = h.tile.plate.color;
                        plateColors[(t * 13) + 7 + vertOffset] = h.tile.plate.color;
                        plateColors[(t * 13) + 8 + vertOffset] = h.tile.plate.color;
                        plateColors[(t * 13) + 9 + vertOffset] = h.tile.plate.color;
                    }
                    else
                    {
                        plateColors[(t * 13) + 0 + vertOffset] = Color.white;
                        plateColors[(t * 13) + 1 + vertOffset] = Color.white;
                        plateColors[(t * 13) + 2 + vertOffset] = Color.white;
                        plateColors[(t * 13) + 3 + vertOffset] = Color.white;
                        plateColors[(t * 13) + 4 + vertOffset] = Color.white;
                        plateColors[(t * 13) + 5 + vertOffset] = Color.white;
                        plateColors[(t * 13) + 6 + vertOffset] = Color.white;
                        plateColors[(t * 13) + 7 + vertOffset] = Color.white;
                        plateColors[(t * 13) + 8 + vertOffset] = Color.white;
                        plateColors[(t * 13) + 9 + vertOffset] = Color.white;
                    }

                    // Triangle One (A - BC - B)
                    mapTris[(t * 36) + 0 + triOffset] = (t * 13) + 0 + vertOffset;
                    mapTris[(t * 36) + 1 + triOffset] = (t * 13) + 3 + vertOffset;
                    mapTris[(t * 36) + 2 + triOffset] = (t * 13) + 1 + vertOffset;

                    // Triangle Two (A - C - BC)
                    mapTris[(t * 36) + 3 + triOffset] = (t * 13) + 0 + vertOffset;
                    mapTris[(t * 36) + 4 + triOffset] = (t * 13) + 2 + vertOffset;
                    mapTris[(t * 36) + 5 + triOffset] = (t * 13) + 3 + vertOffset;

                    // Triangle Three (A - CD - C)
                    mapTris[(t * 36) + 6 + triOffset] = (t * 13) + 0 + vertOffset;
                    mapTris[(t * 36) + 7 + triOffset] = (t * 13) + 5 + vertOffset;
                    mapTris[(t * 36) + 8 + triOffset] = (t * 13) + 2 + vertOffset;

                    // Triangle Four (A - D - CD)
                    mapTris[(t * 36) + 9 + triOffset] = (t * 13) + 0 + vertOffset;
                    mapTris[(t * 36) + 10 + triOffset] = (t * 13) + 4 + vertOffset;
                    mapTris[(t * 36) + 11 + triOffset] = (t * 13) + 5 + vertOffset;

                    // Triangle Five (A - DE - D)
                    mapTris[(t * 36) + 12 + triOffset] = (t * 13) + 0 + vertOffset;
                    mapTris[(t * 36) + 13 + triOffset] = (t * 13) + 7 + vertOffset;
                    mapTris[(t * 36) + 14 + triOffset] = (t * 13) + 4 + vertOffset;

                    // Triangle Six (A - E - DE)
                    mapTris[(t * 36) + 15 + triOffset] = (t * 13) + 0 + vertOffset;
                    mapTris[(t * 36) + 16 + triOffset] = (t * 13) + 6 + vertOffset;
                    mapTris[(t * 36) + 17 + triOffset] = (t * 13) + 7 + vertOffset;

                    // Triangle Seven (A - EF - E)
                    mapTris[(t * 36) + 18 + triOffset] = (t * 13) + 0 + vertOffset;
                    mapTris[(t * 36) + 19 + triOffset] = (t * 13) + 9 + vertOffset;
                    mapTris[(t * 36) + 20 + triOffset] = (t * 13) + 6 + vertOffset;

                    // Triangle Eight (A - F - EF)
                    mapTris[(t * 36) + 21 + triOffset] = (t * 13) + 0 + vertOffset;
                    mapTris[(t * 36) + 22 + triOffset] = (t * 13) + 8 + vertOffset;
                    mapTris[(t * 36) + 23 + triOffset] = (t * 13) + 9 + vertOffset;

                    // Recheck listings.
                    if (!h.pent)
                    {
                        mapVerts[(t * 13) + 10 + vertOffset] = h.vertices[5].pos;                                // G
                        mapVerts[(t * 13) + 11 + vertOffset] = (h.vertices[4].pos + h.vertices[5].pos) / 2.0f;  // FG
                        mapVerts[(t * 13) + 12 + vertOffset] = (h.vertices[5].pos + h.vertices[0].pos) / 2.0f;  // GB

                        mapUV[(t * 13) + 10 + vertOffset] = h.uv;
                        mapUV[(t * 13) + 11 + vertOffset] = h.uv;
                        mapUV[(t * 13) + 12 + vertOffset] = h.uv;

                        mapColors[(t * 13) + 10 + vertOffset] = h.color;
                        mapColors[(t * 13) + 11 + vertOffset] = h.color;
                        mapColors[(t * 13) + 12 + vertOffset] = h.color;

                        windColors[(t * 13) + 10 + vertOffset] = h.windColor;
                        windColors[(t * 13) + 11 + vertOffset] = h.windColor;
                        windColors[(t * 13) + 12 + vertOffset] = h.windColor;

                        currentColors[(t * 13) + 10 + vertOffset] = h.currentColor;
                        currentColors[(t * 13) + 11 + vertOffset] = h.currentColor;
                        currentColors[(t * 13) + 12 + vertOffset] = h.currentColor;

                        biomeColors[(t * 13) + 10 + vertOffset] = h.biomeColor;
                        biomeColors[(t * 13) + 11 + vertOffset] = h.biomeColor;
                        biomeColors[(t * 13) + 12 + vertOffset] = h.biomeColor;

                        temperatureColors[(t * 13) + 10 + vertOffset] = h.temperatureColor;
                        temperatureColors[(t * 13) + 11 + vertOffset] = h.temperatureColor;
                        temperatureColors[(t * 13) + 12 + vertOffset] = h.temperatureColor;

                        precipitationColors[(t * 13) + 10 + vertOffset] = h.precipitationColor;
                        precipitationColors[(t * 13) + 11 + vertOffset] = h.precipitationColor;
                        precipitationColors[(t * 13) + 12 + vertOffset] = h.precipitationColor;

                        if (h.tile.plate != null)
                        {
                            plateColors[(t * 13) + 10 + vertOffset] = h.tile.plate.color;
                            plateColors[(t * 13) + 11 + vertOffset] = h.tile.plate.color;
                            plateColors[(t * 13) + 12 + vertOffset] = h.tile.plate.color;
                        }
                        else
                        {
                            plateColors[(t * 13) + 10 + vertOffset] = Color.white;
                            plateColors[(t * 13) + 11 + vertOffset] = Color.white;
                            plateColors[(t * 13) + 12 + vertOffset] = Color.white;
                        }

                        // Triangle Nine (A - FG - F)
                        mapTris[(t * 36) + 24 + triOffset] = (t * 13) + 0 + vertOffset;
                        mapTris[(t * 36) + 25 + triOffset] = (t * 13) + 11 + vertOffset;
                        mapTris[(t * 36) + 26 + triOffset] = (t * 13) + 8 + vertOffset;

                        // Triangle Ten (A - G - FG)
                        mapTris[(t * 36) + 27 + triOffset] = (t * 13) + 0 + vertOffset;
                        mapTris[(t * 36) + 28 + triOffset] = (t * 13) + 10 + vertOffset;
                        mapTris[(t * 36) + 29 + triOffset] = (t * 13) + 11 + vertOffset;

                        // Triangle Eleven (A - GB - G)
                        mapTris[(t * 36) + 30 + triOffset] = (t * 13) + 0 + vertOffset;
                        mapTris[(t * 36) + 31 + triOffset] = (t * 13) + 12 + vertOffset;
                        mapTris[(t * 36) + 32 + triOffset] = (t * 13) + 10 + vertOffset;

                        // Triangle Twelve (A - B - GB)
                        mapTris[(t * 36) + 33 + triOffset] = (t * 13) + 0 + vertOffset;
                        mapTris[(t * 36) + 34 + triOffset] = (t * 13) + 1 + vertOffset;
                        mapTris[(t * 36) + 35 + triOffset] = (t * 13) + 12 + vertOffset;

                        vertOffset = 0;
                        triOffset = 0;
                    }
                    else
                    {
                        mapVerts[(t * 13) + 10 + vertOffset] = (h.vertices[4].pos + h.vertices[0].pos) / 2.0f;  // FB

                        mapUV[(t * 13) + 10 + vertOffset] = h.uv;

                        mapColors[(t * 13) + 10 + vertOffset] = h.color;

                        windColors[(t * 13) + 10 + vertOffset] = h.windColor;

                        currentColors[(t * 13) + 10 + vertOffset] = h.currentColor;

                        biomeColors[(t * 13) + 10 + vertOffset] = h.biomeColor;

                        precipitationColors[(t * 13) + 10 + vertOffset] = h.precipitationColor;

                        temperatureColors[(t * 13) + 10 + vertOffset] = h.temperatureColor;

                        if (h.tile.plate != null)
                        {
                            plateColors[(t * 13) + 10 + vertOffset] = h.tile.plate.color;
                        }
                        else
                        {
                            plateColors[(t * 13) + 10 + vertOffset] = Color.white;
                        }

                        // Triangle Nine (A - FB - F)
                        mapTris[(t * 36) + 24 + triOffset] = (t * 13) + 0 + vertOffset;
                        mapTris[(t * 36) + 25 + triOffset] = (t * 13) + 10 + vertOffset;
                        mapTris[(t * 36) + 26 + triOffset] = (t * 13) + 8 + vertOffset;

                        // Triangle Ten (A - B - FB)
                        mapTris[(t * 36) + 27 + triOffset] = (t * 13) + 0 + vertOffset;
                        mapTris[(t * 36) + 28 + triOffset] = (t * 13) + 1 + vertOffset;
                        mapTris[(t * 36) + 29 + triOffset] = (t * 13) + 10 + vertOffset;

                        vertOffset = -2;
                        triOffset = -6;
                    }
                }

                vMod = hexes.Length * (12 + 1);
                tMod = hexes.Length * (12 * 3);

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

        public void SpawnForests(HexSphereGenerator hGen)
        {
            List<ProceduralGeneration.Tree> continentalPines = new List<ProceduralGeneration.Tree>();
            List<ProceduralGeneration.Tree> subtropicPines = new List<ProceduralGeneration.Tree>();
            List<ProceduralGeneration.Tree> highlandPines = new List<ProceduralGeneration.Tree>();

            for (int i = 0; i < hexes.Length; i++)
            {
                Hex h = hexes[i];

                if (h.tile.biome == Map.Tile.Biome.humidContinental || h.tile.biome == Map.Tile.Biome.humidSubtropic || h.tile.biome == Map.Tile.Biome.highlands)
                {
                    Vector3[] stumpPoints = new Vector3[hGen.worldSettings.treesPerHex];

                    if (h.tile.biome == Map.Tile.Biome.highlands)
                    {
                        stumpPoints = new Vector3[hGen.worldSettings.treesPerHex / 2];
                    }

                    for (int stumps = 0; stumps < stumpPoints.Length; stumps++)
                    {
                        Vertex r1 = h.vertices[Random.Range(0, h.vertices.Length)];
                        Vertex r2 = h.vertices[Random.Range(0, h.vertices.Length)];

                        if (r2 == r1)
                        {
                            r2 = h.center;
                        }

                        stumpPoints[stumps] = Vector3.Lerp(Vector3.Lerp(r1.pos, r2.pos, Random.Range(0.01f, 0.99f)), h.center.pos, 0.1f);
                    }

                    Color canopyColor = new Color(0, 0, 0);

                    if (h.tile.biome == Map.Tile.Biome.humidContinental)
                    {
                        canopyColor = hGen.worldSettings.continentalForestColor;
                    }
                    else if (h.tile.biome == Map.Tile.Biome.humidSubtropic)
                    {
                        canopyColor = hGen.worldSettings.subtropicForestColor;
                    }
                    else
                    {
                        canopyColor = hGen.worldSettings.highlandForestColor;
                    }

                    for (int ind = 0; ind < stumpPoints.Length; ind++)
                    {
                        Vector3 stump = stumpPoints[ind];

                        // Each tree will be made up of a base cube...
                        // and three pyramids sitting on top of one another.
                        // Cube: 8 verts, 12 triangles.
                        // Pyramid: 5 verts, 6 triangles.
                        // Together: 11 verts, 18 triangles.

                        Procedural.ProceduralGeneration.Tree newTree = new Procedural.ProceduralGeneration.Tree(hGen.procSettings, stump, Procedural.ProceduralGeneration.Tree.TreeType.pine, canopyColor);

                        if (h.tile.biome == Map.Tile.Biome.humidContinental)
                        {
                            continentalPines.Add(newTree);
                        }
                        else if (h.tile.biome == Map.Tile.Biome.humidSubtropic)
                        {
                            subtropicPines.Add(newTree);
                        }
                        else
                        {
                            highlandPines.Add(newTree);
                        }
                    }
                }
            }

            if (continentalPines.Count > 0 && continentalPines.Count < 1000)
            {
                GameObject g = Instantiate(forestPrefab, this.transform.position, Quaternion.identity, this.transform);
                g.GetComponent<Map.TreeContainer>().MapTrees(continentalPines);
                g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.continentalForestColor);
            }
            else
            {
                for (int i = 0; i < continentalPines.Count; i += 999)
                {
                    int takeAmount = Mathf.Min(999, continentalPines.Count - i);

                    GameObject g = Instantiate(forestPrefab, this.transform.position, Quaternion.identity, this.transform);
                    g.GetComponent<Map.TreeContainer>().MapTrees(continentalPines.Skip(i).Take(takeAmount).ToList());
                    g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.continentalForestColor);
                }
            }
            if (subtropicPines.Count > 0 && subtropicPines.Count < 1000)
            {
                GameObject g = Instantiate(forestPrefab, this.transform.position, Quaternion.identity, this.transform);
                g.GetComponent<Map.TreeContainer>().MapTrees(subtropicPines);
                g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.subtropicForestColor);
            }
            else
            {
                for (int i = 0; i < subtropicPines.Count; i += 999)
                {
                    int takeAmount = Mathf.Min(999, subtropicPines.Count - i);

                    GameObject g = Instantiate(forestPrefab, this.transform.position, Quaternion.identity, this.transform);
                    g.GetComponent<Map.TreeContainer>().MapTrees(subtropicPines.Skip(i).Take(takeAmount).ToList());
                    g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.subtropicForestColor);
                }
            }
            if (highlandPines.Count > 0 && highlandPines.Count < 1000)
            {
                GameObject g = Instantiate(forestPrefab, this.transform.position, Quaternion.identity, this.transform);
                g.GetComponent<Map.TreeContainer>().MapTrees(highlandPines);
                g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.highlandForestColor);
            }
            else
            {
                for (int i = 0; i < highlandPines.Count; i += 999)
                {
                    int takeAmount = Mathf.Min(999, highlandPines.Count - i);

                    GameObject g = Instantiate(forestPrefab, this.transform.position, Quaternion.identity, this.transform);
                    g.GetComponent<Map.TreeContainer>().MapTrees(highlandPines.Skip(i).Take(takeAmount).ToList());
                    g.GetComponent<MeshRenderer>().material.SetColor("Color_1E143C74", hGen.worldSettings.highlandForestColor);
                }
            }

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
                    Hex newHex = new Hex();
                    newHex.center = new Vertex(v.pos);
                    newHex.color = color;
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

                Hex newHex = new Hex();
                newHex.center = new Vertex(v.pos);
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
                hFound++;
            }
            else if (number == 19)
            {
                Vertex v = new Vertex(new Vector3(0, -hGen.worldRadius, 0));

                Hex newHex = new Hex();
                newHex.center = new Vertex(v.pos);
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
                hFound++;
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

                Vertex a = new Vertex(oldTri.vA.pos);
                a.x = oldTri.vA.x;
                a.y = oldTri.vA.y;

                Vertex ab = new Vertex((oldTri.vA.pos + oldTri.vB.pos) / 2.0f);
                ab.x = oldTri.vB.x;
                ab.y = oldTri.vB.y;

                Vertex b = new Vertex(oldTri.vB.pos);
                b.x = oldTri.vB.x + 1;
                b.y = oldTri.vB.y + flipMod;

                Vertex bc = new Vertex((oldTri.vB.pos + oldTri.vC.pos) / 2.0f);
                bc.x = oldTri.vC.x;
                bc.y = oldTri.vC.y + flipMod;

                Vertex c = new Vertex(oldTri.vC.pos);
                c.x = oldTri.vC.x + 1;
                c.y = oldTri.vC.y;

                Vertex ca = new Vertex((oldTri.vC.pos + oldTri.vA.pos) / 2.0f);
                ca.x = oldTri.vC.x;
                ca.y = oldTri.vC.y;


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
            int v = 0;

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

        Vector3 YRotation(Vector3 p, float ang)
        {
            Vector3 xAxis = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang));
            Vector3 yAxis = new Vector3(0, 1, 0);
            Vector3 zAxis = new Vector3(-Mathf.Sin(ang), 0, Mathf.Cos(ang));

            return RelativeMovement(p, zAxis, yAxis, xAxis);
        }

        public static Vector2 PerpendicularLine(Vector2 vector2)
        {
            return new Vector2(vector2.y, -vector2.x);
        }
        #endregion
    }
}
