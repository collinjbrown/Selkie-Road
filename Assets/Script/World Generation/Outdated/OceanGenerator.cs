using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class OceanGenerator : MonoBehaviour
{
    public Vector3 core;

    public bool useHexes;
    public bool useLinq;

    public int divideDepth;
    public float worldRadius;

    public GameObject chunkPrefab;
    public GameObject planetPrefab;

    public List<Chunk> chunks;
    public Material chunkMaterial;

    List<Vector3> hexCenters;

    Vector3[] allVerts;
    int[] allTris;

    List<Vector3> origins;

    void Start()
    {
        Stopwatch st = new Stopwatch();
        st.Start();

        core = this.gameObject.transform.position;
        origins = new List<Vector3>();

        for (int i = 0; i < icoVerts.Length; i++)
        {
            origins.Add(icoVerts[i]);
        }

        chunks = new List<Chunk>();

        for (int i = 0; i < icoTris.Length; i += 3)
        {
            GameObject g = Instantiate(chunkPrefab, core, Quaternion.identity, this.gameObject.transform);
            g.GetComponent<MeshRenderer>().material = chunkMaterial;

            Chunk c = g.GetComponent<Chunk>();

            c.neighbors = new Chunk[3];

            c.triTris = new int[] { 0, 1, 2 };

            c.triVerts = new Vector3[] { icoVerts[icoTris[i]], icoVerts[icoTris[i + 1]], icoVerts[icoTris[i + 2]] };

            c.color = UnityEngine.Random.ColorHSV();
            // c.color = Color.white;

            chunks.Add(c);
        }

        foreach (Chunk c in chunks)
        {
            int neighborsFound = 0;

            foreach (Chunk n in chunks)
            {
                if (c != n)
                {
                    int sharedVerts = 0;

                    for (int i = 0; i < c.triVerts.Length; i++)
                    {
                        Vector3 v = c.triVerts[i];

                        for (int ni = 0; ni < n.triVerts.Length; ni++)
                        {
                            Vector3 nv = n.triVerts[ni];

                            if (v == nv)
                            {
                                sharedVerts++;
                            }
                        }
                    }

                    if (sharedVerts > 3)
                    {
                        c.neighbors[neighborsFound] = n;
                        neighborsFound++;
                    }
                }
            }
        }

        Generate();

        st.Stop();
        UnityEngine.Debug.Log($"Ocean Generation took {st.ElapsedMilliseconds} milliseconds to run.");
    }

    void Generate()
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

            for (int n = 0; n < origins.Count; n++)
            {
                origins[n] = origins[n].normalized * r;
            }
        }

        // CountVertsAndTris();

        if (useHexes)
        {
            hexCenters = new List<Vector3>();

            // Generate Hexes
            foreach (Chunk c in chunks)
            {
                float h = (c.triVerts[0] - c.triVerts[1]).sqrMagnitude / 2.0f;
                OldGenerateHexes(c, h);
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

        // Create the basic land beneath the ocean.
        // CreateLand();
    }

    public void CreateLand()
    {
        GameObject newPlanet = Instantiate(planetPrefab, core, Quaternion.identity, this.gameObject.transform);
        newPlanet.GetComponent<PlanetGeneration>().worldRadius = worldRadius * 0.8f;
        newPlanet.GetComponent<PlanetGeneration>().Generate();
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





    Vector3 hA = new Vector3(-1, 0, 1);
    Vector3 hB = new Vector3(-0.5f, Mathf.Sqrt(3 / 2), 1);
    Vector3 hC = new Vector3(0.5f, Mathf.Sqrt(3 / 2), 1);
    Vector3 hD = new Vector3(1f, 0, 1);
    Vector3 hE = new Vector3(0.5f, -Mathf.Sqrt(3 / 2), 1);
    Vector3 hF = new Vector3(-0.5f, -Mathf.Sqrt(3 / 2), 1);
    public void OldGenerateHexes(Chunk chunk, float hexWidth)
    {
        Vector3[] oldVerts = chunk.triVerts;

        Vector3[] newVerts = new Vector3[oldVerts.Length * 13];
        int[] newTris = new int[oldVerts.Length * (12 * 3)];

        int vertOffset = 0; // Used to make sure there aren't gaps in the tri array due to pentagons.
        int triOffset = 0;

        for (int i = 0; i < oldVerts.Length; i++)
        {
            Vector3 oldVert = oldVerts[i];

            if (!hexCenters.Contains(oldVert))
            {

                hexCenters.Add(oldVert);

                int desiredNeighbors = 6;

                if (origins.Contains(oldVert))
                {
                    desiredNeighbors = 5;
                }

                Vector3[] unsortedCenters = new Vector3[desiredNeighbors];
                int neighborsFound = 0;

                // neighbor.triTris is coming up null which doesn't seem like it should be possible.

                for (int ci = 0; ci < chunks.Count; ci++)
                {
                    Chunk c = chunks[ci];

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

                    if (checkedNeighbors.Count == 1)
                    {
                        // This is the first pass and thus the one where we need to set the counterclockwise rotation relative...
                        // to the center of the world (or clockwise relative to the outside of the planet).
                        // It seems to always be better to not "useLinq" as it is slightly faster.

                        Vector3 closestNeighborOne = new Vector3(0, 0, 0);
                        Vector3 closestNeighborTwo = new Vector3(0, 0, 0);

                        if (useLinq)
                        {
                            List<Vector3> closestNeighbors = new List<Vector3>();
                            closestNeighbors.AddRange(unsortedCenters);
                            closestNeighbors.Remove(selectNeighbor);
                            closestNeighbors = closestNeighbors.OrderBy((q) => (q - selectNeighbor).sqrMagnitude).ToList();

                            closestNeighborOne = closestNeighbors[0];
                            closestNeighborTwo = closestNeighbors[1];
                        }
                        else
                        {
                            float closestDistanceOne = Mathf.Infinity;

                            for (int s = 0; s < unsortedCenters.Length; s++)
                            {
                                if (!checkedNeighbors.Contains(unsortedCenters[s]))
                                {
                                    if ((unsortedCenters[s] - selectNeighbor).sqrMagnitude < closestDistanceOne)
                                    {
                                        closestDistanceOne = (unsortedCenters[s] - selectNeighbor).sqrMagnitude;
                                        closestNeighborOne = unsortedCenters[s];
                                    }
                                }
                            }

                            float closestDistanceTwo = Mathf.Infinity;

                            for (int s = 0; s < unsortedCenters.Length; s++)
                            {
                                if (!checkedNeighbors.Contains(unsortedCenters[s]) && unsortedCenters[s] != closestNeighborOne)
                                {
                                    if ((unsortedCenters[s] - selectNeighbor).sqrMagnitude < closestDistanceTwo)
                                    {
                                        closestDistanceTwo = (unsortedCenters[s] - selectNeighbor).sqrMagnitude;
                                        closestNeighborTwo = unsortedCenters[s];
                                    }
                                }
                            }
                        }

                        Vector3 crossN = Vector3.Cross((closestNeighborTwo - selectNeighbor), (closestNeighborOne - selectNeighbor));
                        float w = Vector3.Dot(crossN, (selectNeighbor - oldVert));

                        if (w > 0)
                        {
                            closestNeighbor = closestNeighborTwo;
                        }
                        else
                        {
                            closestNeighbor = closestNeighborOne;
                        }
                    }
                    else
                    {
                        for (int s = 0; s < unsortedCenters.Length; s++)
                        {
                            if (!checkedNeighbors.Contains(unsortedCenters[s]))
                            {
                                if ((unsortedCenters[s] - selectNeighbor).sqrMagnitude < closestDistance)
                                {
                                    closestDistance = (unsortedCenters[s] - selectNeighbor).sqrMagnitude;
                                    closestNeighbor = unsortedCenters[s];
                                }
                            }
                        }
                    }

                    selectNeighbor = closestNeighbor;
                    runs++;
                }

                Vector3 hexCenter = new Vector3(0, 0, 0);

                for (int x = 0; x < centersOfNeighbors.Length; x++)
                {
                    hexCenter += centersOfNeighbors[x];
                }

                hexCenter /= desiredNeighbors;

                if (desiredNeighbors == 6)
                {
                    Vector3 vA = newVerts[(i * 13) + 0 + vertOffset] = hexCenter;             // A

                    Vector3 vB = newVerts[(i * 13) + 1 + vertOffset] = centersOfNeighbors[0]; // B

                    Vector3 vC = newVerts[(i * 13) + 2 + vertOffset] = centersOfNeighbors[1]; // C
                    Vector3 vBC = newVerts[(i * 13) + 3 + vertOffset] = (vB + vC) / 2.0f;     // BC

                    Vector3 vD = newVerts[(i * 13) + 4 + vertOffset] = centersOfNeighbors[2]; // D
                    Vector3 vCD = newVerts[(i * 13) + 5 + vertOffset] = (vC + vD) / 2.0f;     // CD

                    Vector3 vE = newVerts[(i * 13) + 6 + vertOffset] = centersOfNeighbors[3]; // E
                    Vector3 vDE = newVerts[(i * 13) + 7 + vertOffset] = (vD + vE) / 2.0f;     // DE

                    Vector3 vF = newVerts[(i * 13) + 8 + vertOffset] = centersOfNeighbors[4]; // F
                    Vector3 vEF = newVerts[(i * 13) + 9 + vertOffset] = (vE + vF) / 2.0f;     // EF

                    Vector3 vG = newVerts[(i * 13) + 10 + vertOffset] = centersOfNeighbors[5];// G
                    Vector3 vFG = newVerts[(i * 13) + 11 + vertOffset] = (vF + vG) / 2.0f;    // FG

                    Vector3 vGB = newVerts[(i * 13) + 12 + vertOffset] = (vG + vB) / 2.0f;    // GB

                    // Triangle One (A - BC - B)
                    newTris[(i * 36) + 0 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 1 + triOffset] = (i * 13) + 3 + vertOffset;
                    newTris[(i * 36) + 2 + triOffset] = (i * 13) + 1 + vertOffset;

                    // Triangle Two (A - C - BC)
                    newTris[(i * 36) + 3 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 4 + triOffset] = (i * 13) + 2 + vertOffset;
                    newTris[(i * 36) + 5 + triOffset] = (i * 13) + 3 + vertOffset;

                    // Triangle Three (A - CD - C)
                    newTris[(i * 36) + 6 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 7 + triOffset] = (i * 13) + 5 + vertOffset;
                    newTris[(i * 36) + 8 + triOffset] = (i * 13) + 2 + vertOffset;

                    // Triangle Four (A - D - CD)
                    newTris[(i * 36) + 9 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 10 + triOffset] = (i * 13) + 4 + vertOffset;
                    newTris[(i * 36) + 11 + triOffset] = (i * 13) + 5 + vertOffset;

                    // Triangle Five (A - DE - D)
                    newTris[(i * 36) + 12 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 13 + triOffset] = (i * 13) + 7 + vertOffset;
                    newTris[(i * 36) + 14 + triOffset] = (i * 13) + 4 + vertOffset;

                    // Triangle Six (A - E - DE)
                    newTris[(i * 36) + 15 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 16 + triOffset] = (i * 13) + 6 + vertOffset;
                    newTris[(i * 36) + 17 + triOffset] = (i * 13) + 7 + vertOffset;

                    // Triangle Seven (A - EF - E)
                    newTris[(i * 36) + 18 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 19 + triOffset] = (i * 13) + 9 + vertOffset;
                    newTris[(i * 36) + 20 + triOffset] = (i * 13) + 6 + vertOffset;

                    // Triangle Eight (A - F - EF)
                    newTris[(i * 36) + 21 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 22 + triOffset] = (i * 13) + 8 + vertOffset;
                    newTris[(i * 36) + 23 + triOffset] = (i * 13) + 9 + vertOffset;

                    // Triangle Nine (A - FG - F)
                    newTris[(i * 36) + 24 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 25 + triOffset] = (i * 13) + 11 + vertOffset;
                    newTris[(i * 36) + 26 + triOffset] = (i * 13) + 8 + vertOffset;

                    // Triangle Ten (A - G - FG)
                    newTris[(i * 36) + 27 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 28 + triOffset] = (i * 13) + 10 + vertOffset;
                    newTris[(i * 36) + 29 + triOffset] = (i * 13) + 11 + vertOffset;

                    // Triangle Eleven (A - GB - G)
                    newTris[(i * 36) + 30 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 31 + triOffset] = (i * 13) + 12 + vertOffset;
                    newTris[(i * 36) + 32 + triOffset] = (i * 13) + 10 + vertOffset;

                    // Triangle Twelve (A - B - GB)
                    newTris[(i * 36) + 33 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 34 + triOffset] = (i * 13) + 1 + vertOffset;
                    newTris[(i * 36) + 35 + triOffset] = (i * 13) + 12 + vertOffset;

                    vertOffset = 0;
                    triOffset = 0;
                }
                else if (desiredNeighbors == 5)
                {
                    Vector3 vA = newVerts[(i * 13) + 0 + vertOffset] = hexCenter;             // A

                    Vector3 vB = newVerts[(i * 13) + 1 + vertOffset] = centersOfNeighbors[0]; // B

                    Vector3 vC = newVerts[(i * 13) + 2 + vertOffset] = centersOfNeighbors[1]; // C
                    Vector3 vBC = newVerts[(i * 13) + 3 + vertOffset] = (vB + vC) / 2.0f;     // BC

                    Vector3 vD = newVerts[(i * 13) + 4 + vertOffset] = centersOfNeighbors[2]; // D
                    Vector3 vCD = newVerts[(i * 13) + 5 + vertOffset] = (vC + vD) / 2.0f;     // CD

                    Vector3 vE = newVerts[(i * 13) + 6 + vertOffset] = centersOfNeighbors[3]; // E
                    Vector3 vDE = newVerts[(i * 13) + 7 + vertOffset] = (vD + vE) / 2.0f;     // DE

                    Vector3 vF = newVerts[(i * 13) + 8 + vertOffset] = centersOfNeighbors[4]; // F
                    Vector3 vEF = newVerts[(i * 13) + 9 + vertOffset] = (vE + vF) / 2.0f;     // EF

                    Vector3 vFB = newVerts[(i * 13) + 10 + vertOffset] = (vF + vB) / 2.0f;    // FB

                    // Triangle One (A - BC - B)
                    newTris[(i * 36) + 0 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 1 + triOffset] = (i * 13) + 3 + vertOffset;
                    newTris[(i * 36) + 2 + triOffset] = (i * 13) + 1 + vertOffset;

                    // Triangle Two (A - BC - C)
                    newTris[(i * 36) + 3 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 4 + triOffset] = (i * 13) + 2 + vertOffset;
                    newTris[(i * 36) + 5 + triOffset] = (i * 13) + 3 + vertOffset;

                    // Triangle Three (A - CD - C)
                    newTris[(i * 36) + 6 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 7 + triOffset] = (i * 13) + 5 + vertOffset;
                    newTris[(i * 36) + 8 + triOffset] = (i * 13) + 2 + vertOffset;

                    // Triangle Four (A - CD - D)
                    newTris[(i * 36) + 9 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 10 + triOffset] = (i * 13) + 4 + vertOffset;
                    newTris[(i * 36) + 11 + triOffset] = (i * 13) + 5 + vertOffset;

                    // Triangle Five (A - DE - D)
                    newTris[(i * 36) + 12 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 13 + triOffset] = (i * 13) + 7 + vertOffset;
                    newTris[(i * 36) + 14 + triOffset] = (i * 13) + 4 + vertOffset;

                    // Triangle Six (A - DE - E)
                    newTris[(i * 36) + 15 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 16 + triOffset] = (i * 13) + 6 + vertOffset;
                    newTris[(i * 36) + 17 + triOffset] = (i * 13) + 7 + vertOffset;

                    // Triangle Seven (A - EF - E)
                    newTris[(i * 36) + 18 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 19 + triOffset] = (i * 13) + 9 + vertOffset;
                    newTris[(i * 36) + 20 + triOffset] = (i * 13) + 6 + vertOffset;

                    // Triangle Eight (A - EF - F)
                    newTris[(i * 36) + 21 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 22 + triOffset] = (i * 13) + 8 + vertOffset;
                    newTris[(i * 36) + 23 + triOffset] = (i * 13) + 9 + vertOffset;

                    // Triangle Nine (A - FB - F)
                    newTris[(i * 36) + 24 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 25 + triOffset] = (i * 13) + 10 + vertOffset;
                    newTris[(i * 36) + 26 + triOffset] = (i * 13) + 8 + vertOffset;

                    // Triangle Ten (A - FB - B)
                    newTris[(i * 36) + 27 + triOffset] = (i * 13) + 0 + vertOffset;
                    newTris[(i * 36) + 28 + triOffset] = (i * 13) + 1 + vertOffset;
                    newTris[(i * 36) + 29 + triOffset] = (i * 13) + 10 + vertOffset;

                    vertOffset = -2;
                    triOffset = -6;
                }
            }
        }


        chunk.hexVerts = newVerts;
        chunk.hexTris = newTris;
    }

    Vector3 RelativeMovement(Vector3 origin, Vector3 p, Vector3 forward, Vector3 up, Vector3 right)
    {
        return new Vector3(p.x * right.x + p.y * up.x + p.z * forward.x + origin.x, p.x * right.y + p.y * up.y + p.z * forward.y + origin.y, p.x * forward.z + p.y * up.z + p.z * forward.z + origin.z);
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
