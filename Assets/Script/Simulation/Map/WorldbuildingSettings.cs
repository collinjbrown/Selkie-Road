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

    public float savannahCutoff = 0.2f; // Earth is around 20.

    public float hotSteppeCutoff = 0.35f; // Earth is around 35.

    public float monsoonCutoff = 0.20f; // Earth is around 20.

    public float mediterraneanCutoff = 0.45f; // Earth is around 30 - 45

    public float humidSubtropicCutoff = 0.45f; // Earth is around 25 - 45

    public float humidContinentalCutoff = 0.45f; // Earth is around 25 - 45

    public float subarcticCutoff = 0.80f; // Earth is around 45

    public float coldHotDesertSplit = 0.6f;

    public float veryHighTemperatureCutoff = 2.0f;
    public float highTemperatureCutoff = 1.0f;
    public float lowTemperatureCutoff = -1.0f;
    public float veryLowTemperatureCutoff = -2.0f;

    public float veryHighPrecipitationCutoff = 2.0f;
    public float highPrecipitationCutoff = 1.0f;
    public float lowPrecipitationCutoff = -1.0f;
    public float veryLowPrecipitationCutoff = -2.0f;
}
