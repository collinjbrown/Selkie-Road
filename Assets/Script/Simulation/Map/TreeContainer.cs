using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.Procedural;

namespace DeadReckoning.Map
{
    public class TreeContainer : MonoBehaviour
    {
        Vector3[] verts;
        int[] tris;

        public void MapTrees(List<ProceduralGeneration.Tree> trees)
        {
            List<Vector3> rawVerts = new List<Vector3>();
            List<int> rawTris = new List<int>();

            foreach (ProceduralGeneration.Tree tree in trees)
            {
                int[] triConversion = tree.triangles;

                for (int i = 0; i < tree.triangles.Length; i++)
                {
                    triConversion[i] = tree.triangles[i] + rawVerts.Count;
                }

                rawTris.AddRange(triConversion);
                rawVerts.AddRange(tree.vertices);
            }

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
            //mesh.RecalculateBounds();
        }
    }
}
