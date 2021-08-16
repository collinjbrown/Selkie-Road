using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;

public class HexSphereGenerator : MonoBehaviour
{
    public bool generate;
    public bool generatePlanet;
    public bool hexes;

    public int subdivideDepth;
    public float oceanDepth;
    public float worldRadius;

    public GameObject hexChunkPrefab;
    public Material chunkMaterial;
    public GameObject planetPrefab;

    [HideInInspector]
    public List<HexChunk> chunks;

    [HideInInspector]
    public Dictionary<Vector3, Hex> vertHexes;

    [HideInInspector]
    public Dictionary<Vector3, List<Triangle>> vecTriangleNeighbors;

    public NoiseSettings noiseSettings;
    NoiseFilter noiseFilter;

    void Start()
    {
        // Starts the generation process (if that's something we want).

        if (generate)
        {
            Stopwatch st = new Stopwatch();
            st.Start();

            Generate();

            st.Stop();
            UnityEngine.Debug.Log($"Ocean generation took {st.ElapsedMilliseconds} milliseconds.");
        }

        if (generatePlanet)
        {
            Stopwatch st = new Stopwatch();
            st.Start();

            GeneratePlanet();

            st.Stop();
            UnityEngine.Debug.Log($"Planet generation took {st.ElapsedMilliseconds} milliseconds.");
        }
    }

    public void Generate()
    {
        // Generates the world.
        vertHexes = new Dictionary<Vector3, Hex>();
        noiseFilter = new NoiseFilter(noiseSettings);

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

            chunks.Add(c);
        }

        for (int d = 0; d < subdivideDepth; d++)
        {
            // Calls each chunk's subdivision method.

            foreach(HexChunk c in chunks)
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
                c.Render(true);
            }
        }
        else
        {
            if (noiseSettings.amplitude != 0 && noiseSettings.period != 0)
            {
                AddNoise();
            }

            foreach (HexChunk c in chunks)
            {
                c.Render(false);
            }
        }
    }

    public void GeneratePlanet()
    {
        GameObject g = Instantiate(planetPrefab, this.gameObject.transform.position, Quaternion.identity, this.transform);

        HexSphereGenerator planetGenerator = g.GetComponent<HexSphereGenerator>();

        planetGenerator.worldRadius = worldRadius * oceanDepth;

        if (!planetGenerator.hexes)
        {
            planetGenerator.noiseSettings = noiseSettings;
        }

        planetGenerator.Generate();
    }

    public void AddNoise()
    {
        foreach (HexChunk c in chunks)
        {
            for (int i = 0; i < c.vertices.Length; i++)
            {
                float elevation = noiseFilter.Evaluate(c.vertices[i].pos);
                Vector3 normal = (this.transform.position - c.vertices[i].pos).normalized;
                c.vertices[i].pos += normal * elevation;
            }
        }
    }

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
}

public class Vertex
{
    public Vector3 pos;
    public List<Vertex> neighbors;

    public Vertex (Vector3 v)
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
    public bool pent;

    public Vertex center;

    public Vertex[] vertices;

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
