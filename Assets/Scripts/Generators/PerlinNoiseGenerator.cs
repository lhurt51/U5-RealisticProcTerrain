using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PerlinParameters
{
    public Vector2 pScale = new Vector2(0.01f, 0.01f);
    public Vector2 pOffset = new Vector2(0.0f, 0.0f);
    public int pSeed;
    public int pOctaves = 3;
    public float pPersistance = 8.0f;
    public float pLacunarity = 2.0f;
    public bool remove = false;

    public void ValidateValues()
    {
        pScale.x = Mathf.Max(pScale.x, 0.01f);
        pScale.y = Mathf.Max(pScale.y, 0.01f);
        pOctaves = Mathf.Max(pOctaves, 1);
        pLacunarity = Mathf.Max(pLacunarity, 1.0f);
        pPersistance = Mathf.Clamp01(pPersistance);
    }
}

public static class PerlinNoiseGenerator {

    public enum NormalizeMode { Local, Global };

    private struct NoiseGenValues
    {
        public PerlinParameters parameter;

        public System.Random prng;
        public Vector2[] octOffsets;

        public NoiseGenValues(AllNoiseValues parent, PerlinParameters parameter, Vector2 center)
        {
            this.parameter = parameter;
            prng = new System.Random(this.parameter.pSeed);
            octOffsets = new Vector2[this.parameter.pOctaves];
            float amplitude = 1.0f;

            for (int i = 0; i < this.parameter.pOctaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + this.parameter.pOffset.x + center.x;
                float offsetY = prng.Next(-100000, 100000) - this.parameter.pOffset.y - center.y;

                octOffsets[i] = new Vector2(offsetX, offsetY);
                // Increase maxValue by amplitude (for avging)
                parent.maxPosHeight += amplitude;
                amplitude *= parameter.pPersistance;
            }

            if (this.parameter.pScale.x <= 0.0f) this.parameter.pScale.x = 0.0001f;
            if (this.parameter.pScale.y <= 0.0f) this.parameter.pScale.y = 0.0001f;
        }
    };

    private struct AllNoiseValues
    {
        public List<NoiseGenValues> noiseList;

        public float halfHeight;
        public float halfWidth;

        public float maxPosHeight;
        public float maxLocalNoiseHeight;
        public float minLocalNoiseHeight;

        public AllNoiseValues(List<PerlinParameters> pList, int hMWidth, int hMHeight, Vector3 center)
        {
            this.halfHeight = hMHeight / 2.0f;
            this.halfWidth = hMWidth / 2.0f;
            this.maxPosHeight = 0.0f;
            this.maxLocalNoiseHeight = float.MinValue;
            this.minLocalNoiseHeight = float.MaxValue;
            noiseList = new List<NoiseGenValues>();

            foreach (PerlinParameters p in pList) noiseList.Add(new NoiseGenValues(this, p, center));
        }
    };

    // PerlinParameters perlinP;

    /* public PerlinParameters PerlinParam
    {
        get { return perlinParam; }
        set { perlinParam = value; }
    } */

    /* List<PerlinParameters> perlinParameters = new List<PerlinParameters>()
    {
        new PerlinParameters()
    }; */

    /* public List<PerlinParameters> PerlinParams
    {
        get { return perlinParameters; }
        set { perlinParameters = value; }
    } */

    /* public float[,] Perlin(float[,] heightMap, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heightMap[x, y] += Utils.fBM((x + perlinP.pOffset.x) * perlinP.pScale.x, (y + perlinP.pOffset.y) * perlinP.pScale.y, perlinP.pOctaves, perlinP.pPersistance, perlinP.pLacunarity) * perlinP.pHeightScale;
            }
        }
        return heightMap;
    } */

    private static float fBM(float x, float y, AllNoiseValues nV, NoiseGenValues nG)
    {
        // Total heigth value
        float noiseHeight = 0.0f;
        // How tall the waves become
        float amplitude = 1.0f;
        // How close the waves are together
        float frequency = 1.0f;

        for (int i = 0; i < nG.parameter.pOctaves; i++)
        {
            float sampleX = (x - nV.halfWidth + nG.octOffsets[i].x) / nG.parameter.pScale.x * frequency;
            float sampleY = (y - nV.halfHeight + nG.octOffsets[i].y) / nG.parameter.pScale.y * frequency;
            // Sample perlin noise with a calculated position based on octaves and a seed
            float perlinVal = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

            // Setting the height equal to that of our perlin value
            noiseHeight += perlinVal * amplitude;
            // Increasing amplitude by persistance (to reduce the amplitude every octave)
            amplitude *= nG.parameter.pPersistance;
            // Increasing the frequency by lacunarity (to increase frequency every octave)
            frequency *= nG.parameter.pLacunarity;
        }

        if (noiseHeight > nV.maxLocalNoiseHeight) nV.maxLocalNoiseHeight = noiseHeight;
        else if (noiseHeight < nV.minLocalNoiseHeight) nV.minLocalNoiseHeight = noiseHeight;

        return noiseHeight;
    }

    public static void AddNewPerlin(List<PerlinParameters> perlinParams)
    {
        perlinParams.Add(new PerlinParameters());
    }

    public static void RemovePerlin(List<PerlinParameters> perlinParams)
    {
        List<PerlinParameters> keptPerlinParameters = new List<PerlinParameters>();

        for (int i = 0; i < perlinParams.Count; i++)
        {
            if (!perlinParams[i].remove) keptPerlinParameters.Add(perlinParams[i]);
        }

        // If there is nothing in the list. Add a new perlin paramater back to the list
        if (keptPerlinParameters.Count == 0) keptPerlinParameters.Add(perlinParams[0]);
        perlinParams = keptPerlinParameters;
    }

    public static float[,] GenPerlinNoise(float[,] heightMap, int width, int height, HeightMapSettings settings, Vector2 sampleCenter)
    {
        if (settings.perlinParams != null && settings.perlinParams.Count > 0)
        {
            AllNoiseValues noiseValues = new AllNoiseValues(settings.perlinParams, width, height, sampleCenter);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    foreach (NoiseGenValues nG in noiseValues.noiseList)
                    {
                        heightMap[x, y] += fBM(x, y, noiseValues, nG);
                    }
                    if (settings.normalizeMode == NormalizeMode.Global)
                    {
                        float normalizedHeight = (heightMap[x, y] + 1) / (noiseValues.maxPosHeight);

                        heightMap[x, y] = Mathf.Clamp(normalizedHeight, 0.0f, int.MaxValue);
                    }
                }
            }

            if (settings.normalizeMode == NormalizeMode.Local)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        heightMap[x, y] = Mathf.InverseLerp(noiseValues.minLocalNoiseHeight, noiseValues.maxLocalNoiseHeight, heightMap[x, y]);
                    }
                }
            }
        }
        
        return heightMap;
    }
}
