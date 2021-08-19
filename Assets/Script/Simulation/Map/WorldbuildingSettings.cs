using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldbuildingSettings
{
    [Range(1, 10)]
    public int tectonicPlates = 7; // Earth has around seven major plates.

    [Range(1, 10000)]
    public int tectonicPushLimit = 30;

    [Range(0.1f, 0.9f)]
    public float hadleyCellCutoff = 0.4f; // Earth is around 30.

    [Range(0.1f, 0.9f)]
    public float ferrelCellCutoff = 0.8f; // Earth is around 60.

    [Range(0f, 1.0f)]
    public float windCurrentCutoff = 0.75f;

    public float rainForestCutoff = 0.1f; // Earth is around 10.

    public float veryHighTemperatureCutoff = 2.0f;
    public float highTemperatureCutoff = 1.0f;
    public float lowTemperatureCutoff = -1.0f;
    public float veryLowTemperatureCutoff = -2.0f;

    public float veryHighPrecipitationCutoff = 2.0f;
    public float highPrecipitationCutoff = 1.0f;
    public float lowPrecipitationCutoff = -1.0f;
    public float veryLowPrecipitationCutoff = -2.0f;

    public Color mountainColor = Color.white;
    public Color rainforestColor = Color.blue;
    public Color desertColor = Color.yellow;
    public Color highlandsColor = Color.Lerp(Color.green, Color.white, 0.25f);
}
