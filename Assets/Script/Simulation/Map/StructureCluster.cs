using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.Procedural;

namespace DeadReckoning.Map
{
    public class StructureCluster : MonoBehaviour
    {
        public int triCount;

        Vector3[] verts = new Vector3[0];
        int[] tris = new int[0];

        public void AddBuilding(ProceduralGeneration.Building newBuilding)
        {
            List<Vector3> rawVerts = new List<Vector3>();
            List<int> rawTris = new List<int>();

            if (verts.Length > 0)
            {
                rawVerts = new List<Vector3>(verts);
            }
            if (tris.Length > 0)
            {
                rawTris = new List<int>(tris);
            }

            int[] triConversion = new int[newBuilding.triangles.Length];

            for (int i = 0; i < newBuilding.triangles.Length; i++)
            {
                triConversion[i] = newBuilding.triangles[i] + rawVerts.Count;
            }

            rawTris.AddRange(triConversion);
            rawVerts.AddRange(newBuilding.vertices);

            verts = rawVerts.ToArray();
            tris = rawTris.ToArray();

            Render();
        }

        void Render()
        {
            Mesh mesh = this.gameObject.GetComponent<MeshFilter>().mesh;

            mesh.Clear();
            mesh.vertices = verts;
            mesh.triangles = tris;

            mesh.RecalculateNormals();
        }
    }
}
