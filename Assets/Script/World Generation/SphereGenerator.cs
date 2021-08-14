using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereGenerator : MonoBehaviour
{
    public bool useHexes;

    public int divideDepth;
    public float worldRadius;

    public GameObject chunkPrefab;

    public List<Chunk> chunks;

    Vector3[] allVerts;
    int[] allTris;

    List<Vector3> origins;

    void Start()
    {
        origins = new List<Vector3>();

        for (int i = 0; i < icoVerts.Length; i++)
        {
            origins.Add(icoVerts[i]);
        }

        chunks = new List<Chunk>();

        for (int i = 0; i < icoTris.Length; i += 3)
        {
            GameObject g = Instantiate(chunkPrefab);
            Chunk c = g.GetComponent<Chunk>();

            c.triTris = new int[] { 0, 1, 2 };

            c.triVerts = new Vector3[] { icoVerts[icoTris[i]], icoVerts[icoTris[i + 1]], icoVerts[icoTris[i + 2]] };

            g.transform.SetParent(this.transform);

            c.color = UnityEngine.Random.ColorHSV();

            chunks.Add(c);
        }

        PrepareSubdivide();
    }

    void PrepareSubdivide()
    {
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

        for (int i = 0; i < origins.Count; i++)
        {
            origins[i] = origins[i].normalized * r;
        }

        CountVertsAndTris();

        if (useHexes)
        {
            // Generate Hexes
            foreach (Chunk c in chunks)
            {
                GenerateHexes(c);
            }

            foreach (Chunk c in chunks)
            {
                RenderWorld(c, c.hexVerts, c.hexTris, true);
            }
        }
        else
        {
            foreach (Chunk c in chunks)
            {
                RenderWorld(c, c.triVerts, c.triTris, true);
            }
        }
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

    public void GenerateHexes(Chunk chunk)
    {
        Vector3[] oldVerts = chunk.triVerts;

        Vector3[] newVerts = new Vector3[oldVerts.Length * 13];
        int[] newTris = new int[oldVerts.Length * (12 * 3)];

        for (int i = 0; i < oldVerts.Length; i++)
        {
            Vector3 oldVert = oldVerts[i];

            int desiredNeighbors = 6;

            if (origins.Contains(oldVert))
            {
                desiredNeighbors = 5;
            }

            Vector3[] unsortedCenters = new Vector3[desiredNeighbors];
            int neighborsFound = 0;

            foreach (Chunk c in chunks)
            {
                for (int v = 0; v < c.triTris.Length; v += 3)
                {
                    if (c.triVerts[c.triTris[v]] == oldVert || c.triVerts[c.triTris[v + 1]] == oldVert || c.triVerts[c.triTris[v + 2]] == oldVert)
                    {
                        unsortedCenters[neighborsFound] = (c.triVerts[c.triTris[v]] + c.triVerts[c.triTris[v + 1]] + c.triVerts[c.triTris[v + 2]]) / 3.0f;
                        neighborsFound++;
                    }
                }
            }

            Vector3[] centersOfNeighbors = new Vector3[desiredNeighbors];

            int runs = 0;

            List<Vector3> checkedNeighbors = new List<Vector3>();
            Vector3 selectNeighbor = unsortedCenters[Random.Range(0, unsortedCenters.Length)];

            while (runs < desiredNeighbors)
            {
                checkedNeighbors.Add(selectNeighbor);
                centersOfNeighbors[runs] = selectNeighbor;

                Vector3 closestNeighbor = new Vector3(0, 0, 0);
                float closestDistance = Mathf.Infinity;

                for (int s = 0; s < unsortedCenters.Length; s++)
                {
                    if (!checkedNeighbors.Contains(unsortedCenters[s]))
                    {
                        if (Vector3.Distance(selectNeighbor, unsortedCenters[s]) < closestDistance)
                        {
                            closestDistance = Vector3.Distance(selectNeighbor, unsortedCenters[s]);
                            closestNeighbor = unsortedCenters[s];
                        }
                    }
                }

                selectNeighbor = closestNeighbor;
                runs++;
            }

            //for (int t = 0; t < allTris.Length; t += 3)
            //{
            //    if (allVerts[allTris[t]] == oldVert || allVerts[allTris[t + 1]] == oldVert || allVerts[allTris[t + 2]] == oldVert)
            //    {
            //        centersOfNeighbors[neighborsFound] = (allVerts[allTris[t]] + allVerts[allTris[t + 1]] + allVerts[allTris[t + 2]]) / 3.0f;
            //        neighborsFound++;
            //    }
            //}

            if (neighborsFound != desiredNeighbors)
            {
                Debug.LogError($"Wrong Amount of Neighbors: {neighborsFound}");
                Debug.LogError($"Wrong Center: {oldVert}");
            }

            float f = neighborsFound;
            Vector3 hexCenter = new Vector3(0, 0, 0);

            for (int x = 0; x < centersOfNeighbors.Length; x++)
            {
                hexCenter += centersOfNeighbors[x];
            }

            hexCenter /= f;

            if (desiredNeighbors == 6)
            {
                Vector3 vA = newVerts[(i * 13) + 0] = hexCenter;             // A

                Vector3 vB = newVerts[(i * 13) + 1] = centersOfNeighbors[0]; // B

                Vector3 vC = newVerts[(i * 13) + 2] = centersOfNeighbors[1]; // C
                Vector3 vBC = newVerts[(i * 13) + 3] = (vB + vC) / 2.0f;     // BC

                Vector3 vD = newVerts[(i * 13) + 4] = centersOfNeighbors[2]; // D
                Vector3 vCD = newVerts[(i * 13) + 5] = (vC + vD) / 2.0f;     // CD

                Vector3 vE = newVerts[(i * 13) + 6] = centersOfNeighbors[3]; // E
                Vector3 vDE = newVerts[(i * 13) + 7] = (vD + vE) / 2.0f;     // DE

                Vector3 vF = newVerts[(i * 13) + 8] = centersOfNeighbors[4]; // F
                Vector3 vEF = newVerts[(i * 13) + 9] = (vE + vF) / 2.0f;     // EF

                Vector3 vG = newVerts[(i * 13) + 10] = centersOfNeighbors[5];// G
                Vector3 vFG = newVerts[(i * 13) + 11] = (vF + vG) / 2.0f;    // FG

                Vector3 vGB = newVerts[(i * 13) + 12] = (vG + vB) / 2.0f;    // GB

                // Triangle One (A - BC - B)
                newTris[(i * 36) + 0] = (i * 13) + 0;
                newTris[(i * 36) + 1] = (i * 13) + 3;
                newTris[(i * 36) + 2] = (i * 13) + 1;

                // Triangle Two (A - BC - C)
                newTris[(i * 36) + 3] = (i * 13) + 0;
                newTris[(i * 36) + 4] = (i * 13) + 2;
                newTris[(i * 36) + 5] = (i * 13) + 3;

                // Triangle Three (A - CD - C)
                newTris[(i * 36) + 6] = (i * 13) + 0;
                newTris[(i * 36) + 7] = (i * 13) + 5;
                newTris[(i * 36) + 8] = (i * 13) + 2;

                // Triangle Four (A - CD - D)
                newTris[(i * 36) + 9] = (i * 13) + 0;
                newTris[(i * 36) + 10] = (i * 13) + 4;
                newTris[(i * 36) + 11] = (i * 13) + 5;

                // Triangle Five (A - DE - D)
                newTris[(i * 36) + 12] = (i * 13) + 0;
                newTris[(i * 36) + 13] = (i * 13) + 7;
                newTris[(i * 36) + 14] = (i * 13) + 4;

                // Triangle Six (A - DE - E)
                newTris[(i * 36) + 15] = (i * 13) + 0;
                newTris[(i * 36) + 16] = (i * 13) + 6;
                newTris[(i * 36) + 17] = (i * 13) + 7;

                // Triangle Seven (A - EF - E)
                newTris[(i * 36) + 18] = (i * 13) + 0;
                newTris[(i * 36) + 19] = (i * 13) + 9;
                newTris[(i * 36) + 20] = (i * 13) + 6;

                // Triangle Eight (A - EF - F)
                newTris[(i * 36) + 21] = (i * 13) + 0;
                newTris[(i * 36) + 22] = (i * 13) + 8;
                newTris[(i * 36) + 23] = (i * 13) + 9;

                // Triangle Nine (A - FG - F)
                newTris[(i * 36) + 24] = (i * 13) + 0;
                newTris[(i * 36) + 25] = (i * 13) + 11;
                newTris[(i * 36) + 26] = (i * 13) + 8;

                // Triangle Ten (A - FG - G)
                newTris[(i * 36) + 27] = (i * 13) + 0;
                newTris[(i * 36) + 28] = (i * 13) + 10;
                newTris[(i * 36) + 29] = (i * 13) + 11;

                // Triangle Eleven (A - GB - G)
                newTris[(i * 36) + 30] = (i * 13) + 0;
                newTris[(i * 36) + 31] = (i * 13) + 12;
                newTris[(i * 36) + 32] = (i * 13) + 10;

                // Triangle Twelve (A - GB - B)
                newTris[(i * 36) + 33] = (i * 13) + 0;
                newTris[(i * 36) + 34] = (i * 13) + 1;
                newTris[(i * 36) + 35] = (i * 13) + 12;
            }
            else if (desiredNeighbors == 5)
            {
                Vector3 vA = newVerts[(i * 13) + 0] = hexCenter;             // A

                Vector3 vB = newVerts[(i * 13) + 1] = centersOfNeighbors[0]; // B

                Vector3 vC = newVerts[(i * 13) + 2] = centersOfNeighbors[1]; // C
                Vector3 vBC = newVerts[(i * 13) + 3] = (vB + vC) / 2.0f;     // BC

                Vector3 vD = newVerts[(i * 13) + 4] = centersOfNeighbors[2]; // D
                Vector3 vCD = newVerts[(i * 13) + 5] = (vC + vD) / 2.0f;     // CD

                Vector3 vE = newVerts[(i * 13) + 6] = centersOfNeighbors[3]; // E
                Vector3 vDE = newVerts[(i * 13) + 7] = (vD + vE) / 2.0f;     // DE

                Vector3 vF = newVerts[(i * 13) + 8] = centersOfNeighbors[4]; // F
                Vector3 vEF = newVerts[(i * 13) + 9] = (vE + vF) / 2.0f;     // EF

                Vector3 vFB = newVerts[(i * 13) + 10] = (vF + vB) / 2.0f;    // FB

                // Triangle One (A - BC - B)
                newTris[(i * 36) + 0] = (i * 13) + 0;
                newTris[(i * 36) + 1] = (i * 13) + 3;
                newTris[(i * 36) + 2] = (i * 13) + 1;

                // Triangle Two (A - BC - C)
                newTris[(i * 36) + 3] = (i * 13) + 0;
                newTris[(i * 36) + 4] = (i * 13) + 2;
                newTris[(i * 36) + 5] = (i * 13) + 3;

                // Triangle Three (A - CD - C)
                newTris[(i * 36) + 6] = (i * 13) + 0;
                newTris[(i * 36) + 7] = (i * 13) + 5;
                newTris[(i * 36) + 8] = (i * 13) + 2;

                // Triangle Four (A - CD - D)
                newTris[(i * 36) + 9] = (i * 13) + 0;
                newTris[(i * 36) + 10] = (i * 13) + 4;
                newTris[(i * 36) + 11] = (i * 13) + 5;

                // Triangle Five (A - DE - D)
                newTris[(i * 36) + 12] = (i * 13) + 0;
                newTris[(i * 36) + 13] = (i * 13) + 7;
                newTris[(i * 36) + 14] = (i * 13) + 4;

                // Triangle Six (A - DE - E)
                newTris[(i * 36) + 15] = (i * 13) + 0;
                newTris[(i * 36) + 16] = (i * 13) + 6;
                newTris[(i * 36) + 17] = (i * 13) + 7;

                // Triangle Seven (A - EF - E)
                newTris[(i * 36) + 18] = (i * 13) + 0;
                newTris[(i * 36) + 19] = (i * 13) + 9;
                newTris[(i * 36) + 20] = (i * 13) + 6;

                // Triangle Eight (A - EF - F)
                newTris[(i * 36) + 21] = (i * 13) + 0;
                newTris[(i * 36) + 22] = (i * 13) + 8;
                newTris[(i * 36) + 23] = (i * 13) + 9;

                // Triangle Nine (A - FB - F)
                newTris[(i * 36) + 24] = (i * 13) + 0;
                newTris[(i * 36) + 25] = (i * 13) + 10;
                newTris[(i * 36) + 26] = (i * 13) + 8;

                // Triangle Ten (A - FB - B)
                newTris[(i * 36) + 27] = (i * 13) + 0;
                newTris[(i * 36) + 28] = (i * 13) + 1;
                newTris[(i * 36) + 29] = (i * 13) + 10;
            }

        }

        chunk.hexVerts = newVerts;
        chunk.hexTris = newTris;
    }

    
    public void CountVertsAndTris()
    {
        int cV = 0;
        int cT = 0;

        foreach (Chunk c in chunks)
        {
            cV += c.triVerts.Length;
            cT += c.triTris.Length;
        }

        allVerts = new Vector3[cV];
        allTris = new int[cT];

        cV = 0;
        cT = 0;

        foreach (Chunk c in chunks)
        {
            for (int i = 0; i < c.triVerts.Length; i++)
            {
                allVerts[i + cV] = c.triVerts[i];
            }

            for (int i = 0; i < c.triTris.Length; i++)
            {
                int nI = c.triTris[i] + cV;
                allTris[i + cT] = nI;
            }

            cV += c.triVerts.Length;
            cT += c.triTris.Length;
        }
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
