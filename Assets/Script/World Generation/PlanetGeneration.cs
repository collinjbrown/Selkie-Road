using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class PlanetGeneration : MonoBehaviour
{
    public Vector3 core;

    public int divideDepth;
    public float worldRadius;
    public float noiseScale;

    public GameObject chunkPrefab;

    public List<Chunk> chunks;
    public Material chunkMaterial;

    public void Generate()
    {
        core = this.gameObject.transform.position;

        chunks = new List<Chunk>();

        for (int i = 0; i < icoTris.Length; i += 3)
        {
            GameObject g = Instantiate(chunkPrefab, core, Quaternion.identity, this.gameObject.transform);
            g.GetComponent<MeshRenderer>().material = chunkMaterial;

            Chunk c = g.GetComponent<Chunk>();

            c.triTris = new int[] { 0, 1, 2 };

            c.triVerts = new Vector3[] { icoVerts[icoTris[i]], icoVerts[icoTris[i + 1]], icoVerts[icoTris[i + 2]] };

            c.color = UnityEngine.Random.ColorHSV();
            // c.color = Color.white;

            chunks.Add(c);
        }

        Stopwatch st = new Stopwatch();
        st.Start();

        // Subdivide
        int d = divideDepth;

        if (d > 6)
        {
            d = 6;
        }
        else if (d < 0)
        {
            d = 0;
        }

        float r = worldRadius;

        if (r > 100)
        {
            r = 100f;
        }
        else if (r < 1)
        {
            r = 1;
        }

        for (int i = 0; i < d; i++)
        {
            foreach (Chunk c in chunks)
            {
                Subdivide(c, r);
            }
        }

        foreach (Chunk c in chunks)
        {
            Noisify(c);
        }

        foreach (Chunk c in chunks)
        {
            RenderWorld(c, c.triVerts, c.triTris, true);
        }

        st.Stop();
        UnityEngine.Debug.Log($"World Generation took {st.ElapsedMilliseconds} milliseconds to run.");
    }

    public void Noisify(Chunk c)
    {
        Dictionary<Vector3, float> cohesionInsurance = new Dictionary<Vector3, float>();

        float ns = noiseScale;

        if (ns <= 0)
        {
            ns = 0.0001f;
        }

        for (int i = 0; i < c.triVerts.Length; i++)
        {
            if (!cohesionInsurance.ContainsKey(c.triVerts[i]))
            {
                float noiseHeight = Perlin3D(c.triVerts[i]);
                cohesionInsurance.Add(c.triVerts[i], noiseHeight);
                c.triVerts[i] *= noiseHeight / ns;
            }
            else
            {
                c.triVerts[i] *= cohesionInsurance[c.triVerts[i]] / ns;
            }
        }
    }

    public static float Perlin3D(Vector3 vect)
    {
        float x = vect.x;
        float y = vect.y;
        float z = vect.z;

        float ab = Mathf.PerlinNoise(x, y);
        float bc = Mathf.PerlinNoise(y, z);
        float ac = Mathf.PerlinNoise(x, z);

        float ba = Mathf.PerlinNoise(y, x);
        float cb = Mathf.PerlinNoise(z, y);
        float ca = Mathf.PerlinNoise(z, x);

        float abc = ab + bc + ac + ba + cb + ca;
        return abc / 6.0f;
    }

    public void Subdivide(Chunk chunk, float rad)
    {
        Vector3[] oldVerts = chunk.triVerts;
        int[] oldTris = chunk.triTris;

        Vector3[] newVerts = new Vector3[oldTris.Length * 2];
        int[] newTris = new int[oldTris.Length * 4];


        // Generate subdivisions with duplicates.
        int o = 0;

        for (int i = 0; i < oldTris.Length; i += 3)
        {

            // Generate new vertices.
            newVerts[(o * 6) + 0] = oldVerts[oldTris[i]];                                                     // A
            newVerts[(o * 6) + 0] = newVerts[(o * 6) + 0].normalized * rad;

            newVerts[(o * 6) + 1] = (oldVerts[oldTris[i + 1]] + oldVerts[oldTris[i]]) / 2.0f;                 // AB
            newVerts[(o * 6) + 1] = newVerts[(o * 6) + 1].normalized * rad;

            newVerts[(o * 6) + 2] = oldVerts[oldTris[i + 1]];                                                 // B
            newVerts[(o * 6) + 2] = newVerts[(o * 6) + 2].normalized * rad;

            newVerts[(o * 6) + 3] = (oldVerts[oldTris[i + 2]] + oldVerts[oldTris[i + 1]]) / 2.0f;             // BC
            newVerts[(o * 6) + 3] = newVerts[(o * 6) + 3].normalized * rad;

            newVerts[(o * 6) + 4] = oldVerts[oldTris[i + 2]];                                                 // C
            newVerts[(o * 6) + 4] = newVerts[(o * 6) + 4].normalized * rad;

            newVerts[(o * 6) + 5] = (oldVerts[oldTris[i + 2]] + oldVerts[oldTris[i]]) / 2.0f;                 // CA
            newVerts[(o * 6) + 5] = newVerts[(o * 6) + 5].normalized * rad;


            // Generate new triangles.
            newTris[(o * 12) + 0] = (o * 6) + 0;  // Triangle One
            newTris[(o * 12) + 1] = (o * 6) + 1;
            newTris[(o * 12) + 2] = (o * 6) + 5;

            newTris[(o * 12) + 3] = (o * 6) + 1;  // Triangle Two
            newTris[(o * 12) + 4] = (o * 6) + 2;
            newTris[(o * 12) + 5] = (o * 6) + 3;

            newTris[(o * 12) + 6] = (o * 6) + 5;  // Triangle Three
            newTris[(o * 12) + 7] = (o * 6) + 3;
            newTris[(o * 12) + 8] = (o * 6) + 4;

            newTris[(o * 12) + 9] = (o * 6) + 1;  // Triangle Four
            newTris[(o * 12) + 10] = (o * 6) + 3;
            newTris[(o * 12) + 11] = (o * 6) + 5;

            o++;
        }

        chunk.triVerts = newVerts;
        chunk.triTris = newTris;
    }

    public void RenderWorld(Chunk chunk, Vector3[] verts, int[] tris, bool optimize)
    {
        Mesh mesh = chunk.GetComponent<MeshFilter>().mesh;
        chunk.gameObject.GetComponent<MeshRenderer>().material.color = chunk.color;

        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = tris;
        if (optimize)
        {
            mesh.Optimize();
        }
        mesh.RecalculateNormals();

        MeshCollider meshCol = chunk.GetComponent<MeshCollider>();
        meshCol.sharedMesh = new Mesh();
        meshCol.sharedMesh.vertices = verts;
        meshCol.sharedMesh.triangles = tris;
        meshCol.sharedMesh.RecalculateBounds();
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
