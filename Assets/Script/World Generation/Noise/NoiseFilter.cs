using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseFilter
{
    NoiseSettings settings;
    Noise noise = new Noise();

    public float Evaluate(Vector3 point)
    {
        if (settings.seed != 0)
        {
            noise = new Noise(settings.seed);
        }
        else
        {
            settings.seed = Random.Range(100000, 1000000);
            noise = new Noise(settings.seed);
        }

        float noiseValue = 0;
        float frequency = settings.baseRoughness;
        float amplitude = 1;

        for (int i = 0; i < settings.numLayers; i++)
        {
            float v = noise.Evaluate(point * frequency + settings.center);
            noiseValue += (v + 1) * 0.5f * amplitude;
            frequency *= settings.roughness;
            amplitude *= settings.persistence;
        }

        noiseValue = Mathf.Max(0, noiseValue - settings.minValue);
        noiseValue = Mathf.Min(noiseValue, settings.maxValue);
        return noiseValue * settings.strength;
    }

    public NoiseFilter(NoiseSettings ns)
    {
        settings = ns;
    }
}
