using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassTester : MonoBehaviour
{
    Vector3 v1 = new Vector3(0, 0, 0);
    Vector3 v2 = new Vector3(0, 1, 0);
    Vector3 v3 = new Vector3(1, 1, 0);

    // Start is called before the first frame update
    void Start()
    {

        Mesh mesh = this.gameObject.GetComponent<MeshFilter>().mesh;

        mesh.vertices = new Vector3[] { v1, v2, v3 };
        mesh.triangles = new int[] { 1, 2, 3 };

        mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
