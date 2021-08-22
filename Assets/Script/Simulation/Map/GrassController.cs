using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.WorldGeneration;

public class GrassController : MonoBehaviour
{
    public List<Hex> hexes = new List<Hex>();
    Vector3[] mapVerts;
    int[] mapTris;

    public Color baseColor;
    public Color tipColor;

    public void Render()
    {
        // We need to take all the hexes and map them...
        // to a list or array of vector3s and ints.

        MapVertsandTris();

        this.gameObject.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", baseColor);
        this.gameObject.GetComponent<MeshRenderer>().material.SetColor("_TipColor", tipColor);

        Mesh mesh = this.gameObject.GetComponent<MeshFilter>().mesh;

        mesh.Clear();
        mesh.vertices = mapVerts;
        mesh.triangles = mapTris;

        mesh.Optimize();
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    void MapVertsandTris()
    {
        mapVerts = new Vector3[hexes.Count * (12 + 1)];
        mapTris = new int[hexes.Count * (12 * 3)];

        int vertOffset = 0; // Used to make sure there aren't gaps in the tri array due to pentagons.
        int triOffset = 0;

        for (int t = 0; t < hexes.Count; t++)
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
