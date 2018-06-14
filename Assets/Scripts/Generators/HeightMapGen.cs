using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HeightMap
{
    public readonly float[,] values;
    public readonly float minVal;
    public readonly float maxVal;

    public HeightMap(float[,] values, float minVal, float maxVal)
    {
        this.values = values;
        this.minVal = minVal;
        this.maxVal = maxVal;
    }
}

public class HeightMapGen : MonoBehaviour {

    public static HeightMap GenerateHeightMap(float[,] heightMap, int width, int height, HeightMapSettings settings, Vector2 center)
    {
        float[,] values = PerlinNoiseGenerator.GenPerlinNoise(heightMap, width, height, settings, center);
        AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);
        float minVal = float.MaxValue;
        float maxVal = float.MinValue;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                values[x, y] *= heightCurve_threadsafe.Evaluate(values[x, y]) * settings.heightMultiplier;

                if (values[x, y] > maxVal) maxVal = values[x, y];
                else if (values[x, y] < minVal) minVal = values[x, y];
            }
        }

        return new HeightMap(values, minVal, maxVal);
    }
}
