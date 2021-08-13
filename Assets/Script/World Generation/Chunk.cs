using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Color color;

    public int[] origin;

    [HideInInspector]
    public Vector3[] verts;

    [HideInInspector]
    public int[] tris;
}
