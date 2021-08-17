using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public int seed;

    public float strength = 1; // Strength
    [Range(1,8)]
    public int numLayers = 1;
    public float baseRoughness = 1; // Roughness
    public float roughness = 2;
    public float persistence = 0.5f;
    public Vector3 center;
    public float minValue;
    public float maxValue = 0.75f;
}
