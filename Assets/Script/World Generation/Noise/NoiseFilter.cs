using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseFilter
{
    NoiseSettings settings;
    Noise noise = new Noise();

    public float Evaluate(Vector3 point)
    {
        float noiseValue = (noise.Evaluate(point * settings.period + settings.center) + 1) * 0.5f;
        return noiseValue * settings.amplitude;
    }

    public NoiseFilter(NoiseSettings ns)
    {
        settings = ns;
    }
}
