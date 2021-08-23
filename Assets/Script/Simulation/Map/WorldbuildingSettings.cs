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

    public float polarVariance = 0.05f;
    public float polarCapCutoff = 0.95f;

    public float coldHotDesertSplit = 0.6f;

    public float veryHighTemperatureCutoff = 2.0f;
    public float highTemperatureCutoff = 1.0f;
    public float lowTemperatureCutoff = -1.0f;
    public float veryLowTemperatureCutoff = -2.0f;

    public float veryHighPrecipitationCutoff = 2.0f;
    public float highPrecipitationCutoff = 1.0f;
    public float lowPrecipitationCutoff = -1.0f;
    public float veryLowPrecipitationCutoff = -2.0f;

    #region Mountains
    public bool renderMountains = false;
    public float peakHeight = 0.25f;
    public int peakFacets = 10;
    public Color mountainColor = Color.Lerp(Color.green, Color.red, 0.5f);
    #endregion

    #region Trees
    public bool renderTrees = true;
    public int treesPerHex = 25;
    public Color continentalForestColor = Color.Lerp(Color.green, Color.black, 0.5f);
    public Color subtropicForestColor = Color.Lerp(Color.green, Color.blue, 0.5f);
    public Color highlandForestColor = Color.Lerp(Color.green, Color.blue, 0.5f);
    public Color subarcticForestColor = Color.Lerp(Color.green, Color.blue, 0.75f);
    #endregion

    #region Grasses
    public bool renderGrass = false;

    public Color savannahGrassBaseColor = Color.Lerp(Color.yellow, Color.white, 0.5f);
    public Color savannahGrassTipColor = Color.Lerp(Color.red, Color.white, 0.5f);

    public Color steppeGrassBaseColor = Color.Lerp(Color.yellow, Color.white, 0.5f);
    public Color steppeGrassTipColor = Color.Lerp(Color.yellow, Color.white, 0.75f);

    public Color mediterraneanGrassBaseColor = Color.Lerp(Color.yellow, Color.white, 0.25f);
    public Color mediterraneanGrassTipColor = Color.Lerp(Color.yellow, Color.white, 0.5f);

    public Color oceanicGrassBaseColor = Color.Lerp(Color.green, Color.white, 0.25f);
    public Color oceanicGrassTipColor = Color.Lerp(Color.green, Color.white, 0.5f);

    public Color prairieGrassBaseColor = Color.Lerp(Color.yellow, Color.white, 0.1f);
    public Color prairieGrassTipColor = Color.Lerp(Color.yellow, Color.white, 0.25f);

    public Color highlandGrassBaseColor = Color.Lerp(Color.yellow, Color.red, 0.5f);
    public Color highlandGrassTipColor = Color.Lerp(Color.red, Color.white, 0.5f);
    #endregion
}
