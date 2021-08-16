using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Color color;

    public Chunk[] neighbors;

    [HideInInspector]
    public Vector3[] triVerts;

    [HideInInspector]
    public int[] triTris;

    [HideInInspector]
    public Vector3[] hexVerts;

    [HideInInspector]
    public int[] hexTris;
}
