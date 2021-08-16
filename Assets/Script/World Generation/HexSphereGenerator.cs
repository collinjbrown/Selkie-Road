using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;

public class HexSphereGenerator : MonoBehaviour
{
    public bool generate;
    public bool hexes;

    public int subdivideDepth;
    public float worldRadius;

    public GameObject hexChunkPrefab;
    public Material chunkMaterial;

    [HideInInspector]
    public List<HexChunk> chunks;

    [HideInInspector]
    public Dictionary<Vector3, Hex> vertHexes;

    void Start()
    {
        // Starts the generation process (if that's something we want).

        Stopwatch st = new Stopwatch();
        st.Start();

        if (generate)
        {
            Generate();
        }

        st.Stop();
        UnityEngine.Debug.Log($"New world generation took {st.ElapsedMilliseconds} milliseconds.");
    }

    public void Generate()
    {
        // Generates the world.
        vertHexes = new Dictionary<Vector3, Hex>();

        for (int i = 0; i < icoTris.Length; i += 3)
        {
            // Sets up all the necessary chunks and their values.

            GameObject g = Instantiate(hexChunkPrefab, this.transform.position, Quaternion.identity, this.gameObject.transform);
            g.GetComponent<MeshRenderer>().material = chunkMaterial;

            HexChunk c = g.GetComponent<HexChunk>();

            c.triangles = new Triangle[1];
            c.triangles[0] = new Triangle(new Vertex(icoVerts[icoTris[i]]), new Vertex(icoVerts[icoTris[i + 1]]), new Vertex(icoVerts[icoTris[i + 2]]));
            c.origin = c.triangles[0];

            c.neighbors = c.FindNeighbors(chunks.Count);

            c.number = chunks.Count;
            c.color = Random.ColorHSV();

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
            foreach (HexChunk c in chunks)
            {
                c.Hexify(this);
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
