using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.Procedural;


namespace DeadReckoning.Map
{
    public class ProceduralContainer : MonoBehaviour
    {
        public int vertCount;
        public int triCount;

        Vector3[] verts;
        int[] tris;

        public void MapObjects(List<ProceduralGeneration.RockStructure> objs)
        {
            List<Vector3> rawVerts = new List<Vector3>();
            List<int> rawTris = new List<int>();

            foreach (ProceduralGeneration.RockStructure o in objs)
            {
                int[] triConversion = o.triangles;

                for (int i = 0; i < o.triangles.Length; i++)
                {
                    triConversion[i] = o.triangles[i] + rawVerts.Count;
                }

                rawTris.AddRange(triConversion);
                rawVerts.AddRange(o.vertices);
            }

            verts = rawVerts.ToArray();
            tris = rawTris.ToArray();

            vertCount = rawVerts.Count;
            triCount = rawTris.Count;

            Render();
        }

        public void MapObjects(List<ProceduralGeneration.Tree> objs)
        {
            List<Vector3> rawVerts = new List<Vector3>();
            List<int> rawTris = new List<int>();

            foreach (ProceduralGeneration.Tree o in objs)
            {
                int[] triConversion = o.triangles;

                for (int i = 0; i < o.triangles.Length; i++)
                {
                    triConversion[i] = o.triangles[i] + rawVerts.Count;
                }

                rawTris.AddRange(triConversion);
                rawVerts.AddRange(o.vertices);
            }

            verts = rawVerts.ToArray();
            tris = rawTris.ToArray();

            vertCount = rawVerts.Count;
            triCount = rawTris.Count;

            Render();
        }

        public void MapObjects(List<ProceduralGeneration.ProceduralObject> objs)
        {
            List<Vector3> rawVerts = new List<Vector3>();
            List<int> rawTris = new List<int>();

            foreach (ProceduralGeneration.ProceduralObject o in objs)
            {
                int[] triConversion = o.triangles;

                for (int i = 0; i < o.triangles.Length; i++)
                {
                    triConversion[i] = o.triangles[i] + rawVerts.Count;
                }

                rawTris.AddRange(triConversion);
                rawVerts.AddRange(o.vertices);
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
