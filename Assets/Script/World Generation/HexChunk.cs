using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class HexChunk : MonoBehaviour
{
    public int number;

    public int[] neighbors;

    public Vertex[] vertices;
    public Triangle[] triangles;
    public Hex[] hexes;

    public Dictionary<Vector3, Hex> hexCenters;

    [HideInInspector]
    public Vector3[] mapVerts;

    [HideInInspector]
    public int[] mapTris;

    public Color color;

    public Triangle origin;

    public void Render(bool hex)
    {
        // Displays the chunk in the world.

        MapVertsAndTris(hex);

        Mesh mesh = this.gameObject.GetComponent<MeshFilter>().mesh;
        this.gameObject.GetComponent<MeshRenderer>().material.color = color;

        mesh.Clear();
        mesh.vertices = mapVerts;
        mesh.triangles = mapTris;
        mesh.Optimize();
        mesh.RecalculateNormals();

        MeshCollider meshCol = this.gameObject.GetComponent<MeshCollider>();
        meshCol.sharedMesh = new Mesh();
        meshCol.sharedMesh.vertices = mapVerts;
        meshCol.sharedMesh.triangles = mapTris;
        meshCol.sharedMesh.RecalculateBounds();
    }

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
                Hex newHex = new Hex();
                newHex.center = new Vertex(v.pos);
                hGen.vertHexes.Add(v.pos, newHex);
                newHex.vertices = new Vertex[6];

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

                hexCenters.Add(newHex.center.pos, newHex);
                hexes[hFound] = newHex;
                hFound++;
            }
        }

        // Now we need to add the pole caps.
        // We'll just add these to the first and last chunks.

        if (number == 0)
        {
            Vertex v = new Vertex(new Vector3(0, hGen.worldRadius, 0));

            Hex newHex = new Hex();
            newHex.center = new Vertex(v.pos);
            hGen.vertHexes.Add(v.pos, newHex);
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

            hexCenters.Add(newHex.center.pos, newHex);
            hexes[hFound] = newHex;
            hFound++;
        }
        else if (number == 19)
        {
            Vertex v = new Vertex(new Vector3(0, -hGen.worldRadius, 0));

            Hex newHex = new Hex();
            newHex.center = new Vertex(v.pos);
            hGen.vertHexes.Add(v.pos, newHex);
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

            hexCenters.Add(newHex.center.pos, newHex);
            hexes[hFound] = newHex;
            hFound++;
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

            Vertex a = new Vertex(oldTri.vA.pos);
            Vertex ab = new Vertex((oldTri.vA.pos + oldTri.vB.pos) / 2.0f);
            Vertex b = new Vertex(oldTri.vB.pos);
            Vertex bc = new Vertex((oldTri.vB.pos + oldTri.vC.pos) / 2.0f);
            Vertex c = new Vertex(oldTri.vC.pos);
            Vertex ca = new Vertex((oldTri.vC.pos + oldTri.vA.pos) / 2.0f);

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
            mapVerts = new Vector3[hexes.Length * (12 + 1)];
            mapTris = new int[hexes.Length * (12 * 3)];

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
        }
    }

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

    public Vector3[] FindRelativeAxes(Vertex v)
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
            up = Vector3.forward;
        }
        else if (forward == Vector3.up)
        {
            up = Vector3.back;
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

    Vector3 RelativeMovement(Vector3 p, Vector3 forward, Vector3 up, Vector3 right)
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
}
