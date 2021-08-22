using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.Procedural;


namespace DeadReckoning.Map
{
    public class PeakContainer : MonoBehaviour
    {
        public int vertCount;
        public int triCount;

        Vector3[] verts;
        int[] tris;

        public void MapPeaks(List<ProceduralGeneration.RockStructure> rocks)
        {
            List<Vector3> rawVerts = new List<Vector3>();
            List<int> rawTris = new List<int>();

            foreach (ProceduralGeneration.RockStructure r in rocks)
            {
                int[] triConversion = r.triangles;

                for (int i = 0; i < r.triangles.Length; i++)
                {
                    triConversion[i] = r.triangles[i] + rawVerts.Count;
                }

                rawTris.AddRange(triConversion);
                rawVerts.AddRange(r.vertices);
            }

            verts = rawVerts.ToArray();
            tris = rawTris.ToArray();

            vertCount = rawVerts.Count;
            triCount = rawTris.Count;

            Render();
        }

        void Render()
        {
            Mesh mesh = this.gameObject.GetComponent<MeshFilter>().mesh;

            mesh.Clear();
            mesh.vertices = verts;
            mesh.triangles = tris;

            mesh.RecalculateNormals();
            //mesh.RecalculateBounds();
        }
    }
}
