using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour {
    // Acess to terrain data ---------------------------------------
    public Terrain terrain;
    public TerrainData terrainData;

    // Random Generation --------------------------------------------
    public Vector2 randomHeightRange = new Vector2(0.0f, 0.1f);

    // Texture Loading ----------------------------------------------
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1.0f, 1.0f, 1.0f);

    // Perlin Noise -------------------------------------------------
    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public int perlinOffsetX = 0;
    public int perlinOffsetY = 0;
    public int perlinOctaves = 3;
    public float perlinPersistance = 8.0f;
    public float perlinHeightScale = 0.09f;

    // Multiple perlin ---------------------------------------------
    [System.Serializable]
    public class PerlinParameters
    {
        public float mPerlinXScale = 0.01f;
        public float mPerlinYScale = 0.01f;
        public int mPerlinOffsetX = 0;
        public int mPerlinOffsetY = 0;
        public int mPerlinOctaves = 3;
        public float mPerlinPersistance = 8.0f;
        public float mPerlinHeightScale = 0.09f;
        public bool remove = false;
    }

    public List<PerlinParameters> perlinParameters = new List<PerlinParameters>()
    {
        new PerlinParameters()
    };

    // Voronoi noise -----------------------------------------------
    public int voronoiPeaks = 5;
    public float voronoiMinHeight = 0.0f;
    public float voronoiMaxHeight = 1.0f;
    public float voronoiFallOff = 0.2f;
    public float voronoiDropOff = 0.6f;
    public enum VoronoiType { Linear = 0, Power = 1, Combined = 2, SinPow = 3 }
    public VoronoiType voronoiType = VoronoiType.Linear;

    // Midpoint Displacement ---------------------------------------
    public float MPDHeightMin = -2.0f;
    public float MPDHeightMax = 2.0f;
    public float MPDHeightDampenerPower = 2.0f;
    public float MPDRoughness = 2.0f;

    // Splatmaps ---------------------------------------------------
    [System.Serializable]
    public class SplatHeights
    {
        public Texture2D texture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0.0f;
        public float maxSlope = 90.0f;
        public Vector2 tileOffset = new Vector2(0.0f, 0.0f);
        public Vector2 tileSize = new Vector2(50.0f, 50.0f);
        public float splatBlendOffset = 0.01f;
        public Vector2 splatNoiseVScale = new Vector2(0.01f, 0.01f);
        public float splatNoiseScaler = 0.1f;
        public bool remove = false;
    }

    public List<SplatHeights> splatHeights = new List<SplatHeights>()
    {
        new SplatHeights()
    };

    // Smooth Algo -------------------------------------------------
    public int smoothAmount = 5;

    // Should it reset the terrain before generating a new height map
    public bool resetBeforeGen;

    // Private Class Methods ----------------------------------------
    private void AddTag(SerializedProperty tagsProp, string newTag)
    {
        bool found = false;

        // Ensure the tag doesn't already exist
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newTag)) { found = true; break; }
        }

        // Add the new tag
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }
    }

    private void OnEnable()
    {
        Debug.Log("Initialising Terrain Data");
        // Grabing the terrain component from the attached object
        terrain = this.GetComponent<Terrain>();
        // Setting our terrain data tot the terrain components terrain data
        terrainData = Terrain.activeTerrain.terrainData;
    }

    private void Awake()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        AddTag(tagsProp, "Terrain");
        AddTag(tagsProp, "Cloud");
        AddTag(tagsProp, "Shore");

        // Apply tag changes to the tag database
        tagManager.ApplyModifiedProperties();

        // Take this object
        this.gameObject.tag = "Terrain";
    }

    private float[,] GetHeightMapChoice()
    {
        if (!resetBeforeGen) return terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        else return new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
    }

    private void NormalizeVector(float[] v)
    {
        float total = 0;

        for (int i = 0; i < v.Length; i++) total += v[i];

        for (int i = 0; i < v.Length; i++) v[i] /= total;
    }

    private List<Vector2> GenerateNeighbours(Vector2 pos, int width, int height)
    {
        List<Vector2> neighbours = new List<Vector2>();

        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if (!(x == 0 && y == 0))
                {
                    Vector2 nPos = new Vector2(Mathf.Clamp(pos.x + x, 0.0f, width - 1), Mathf.Clamp(pos.y + y, 0.0f, height - 1));

                    if (!neighbours.Contains(nPos)) neighbours.Add(nPos);
                }
            }
        }
        return neighbours;
    }

    private float GetSteepness(float[,] heightMap, int x, int y, int width, int height)
    {
        float h = heightMap[x, y];
        int nx = x + 1;
        int ny = y + 1;

        // If on the upper edge of the map find gradient by going backward
        if (nx > width - 1) nx = x - 1;
        if (ny > height - 1) ny = y - 1;

        float dx = heightMap[nx, y] - h;
        float dy = heightMap[x, ny] - h;
        Vector2 gradient = new Vector2(dx, dy);

        float steep = gradient.magnitude;

        return steep;
    }

    // Public Class Methods ------------------------------------------
    public float[,] GetHeightMap()
    {
        return terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
    }

    public void RandomTerrain()
    {
        float[,] heightMap = GetHeightMapChoice();

        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                heightMap[x, y] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        // Applying the height map to the terrain at position (0, 0)
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void LoadTexture()
    {
        float[,] heightMap = GetHeightMapChoice();

        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < terrainData.heightmapHeight; z++)
            {
                heightMap[x, z] += heightMapImage.GetPixel((int)(x * heightMapScale.x), (int)(z * heightMapScale.z)).grayscale * heightMapScale.y;
            }
        }
        // Applying the height map to the terrain at position (0, 0)
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void Perlin()
    {
        float[,] heightMap = GetHeightMapChoice();

        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                heightMap[x, y] += Utils.fBM((x + perlinOffsetX) * perlinXScale, (y + perlinOffsetY) * perlinYScale, perlinOctaves, perlinPersistance) * perlinHeightScale;
            }
        }
        // Applying the height map to the terrain at position (0, 0)
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void AddNewPerlin()
    {
        perlinParameters.Add(new PerlinParameters());
    }

    public void RemovePerlin()
    {
        List<PerlinParameters> keptPerlinParameters = new List<PerlinParameters>();

        for (int i = 0; i < perlinParameters.Count; i++)
        {
            if (!perlinParameters[i].remove)
            {
                keptPerlinParameters.Add(perlinParameters[i]);
            }
        }

        // If there is nothing in the list. Add the first perlin parameter back to the list
        if (keptPerlinParameters.Count == 0) keptPerlinParameters.Add(perlinParameters[0]);
        perlinParameters = keptPerlinParameters;
    }

    public void MultiplePerlinTerrain()
    {
        float[,] heightMap = GetHeightMapChoice();

        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                foreach (PerlinParameters p in perlinParameters)
                {
                    heightMap[x, y] += Utils.fBM((x + p.mPerlinOffsetX) * p.mPerlinXScale, (y + p.mPerlinOffsetY) * p.mPerlinYScale, p.mPerlinOctaves, p.mPerlinPersistance) * p.mPerlinHeightScale;
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void Voronoi()
    {
        float[,] heightMap = GetHeightMapChoice();

        for (int p = 0; p < voronoiPeaks; p++)
        {
            // Defining where the peak is
            Vector3 peak = new Vector3(UnityEngine.Random.Range(0.0f, terrainData.heightmapWidth), UnityEngine.Random.Range(voronoiMinHeight, voronoiMaxHeight), UnityEngine.Random.Range(0.0f, terrainData.heightmapHeight));
            Vector2 peakLoc = new Vector2(peak.x, peak.z);
            // Finding max distance for averaging distances
            float maxDist = Vector2.Distance(new Vector2(0, 0), new Vector2(terrainData.heightmapWidth, terrainData.heightmapHeight));

            // Making sure the peak is still visible
            if (heightMap[(int)peak.x, (int)peak.z] >= peak.y) continue;
            // Setting the height maps peak
            heightMap[(int)peak.x, (int)peak.z] = peak.y;
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                for (int x = 0; x < terrainData.heightmapWidth; x++)
                {
                    // Making sure we skip the peak
                    if (!(x == peak.x && y == peak.z))
                    {
                        float dstToPeak = Vector2.Distance(peakLoc, new Vector2(x, y)) / maxDist;
                        float h;

                        if (voronoiType == VoronoiType.SinPow) h = peak.y - Mathf.Pow(dstToPeak * 3.0f, voronoiFallOff) - Mathf.Sin(dstToPeak * 2.0f * Mathf.PI) / voronoiDropOff;
                        else if (voronoiType == VoronoiType.Combined) h = peak.y - dstToPeak * voronoiFallOff - Mathf.Pow(dstToPeak, voronoiDropOff);
                        else if (voronoiType == VoronoiType.Power) h = peak.y - Mathf.Pow(dstToPeak, voronoiDropOff) * voronoiFallOff;
                        else h = peak.y - dstToPeak * voronoiFallOff;
                        // Sin wave code....
                        // h = peak.y - Mathf.Sin(dstToPeak * 200 * Mathf.PI) * 0.01f;
                        if (heightMap[x, y] < h) heightMap[x, y] = h;
                    }
                }
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void MPD()
    {
        float[,] heightMap = GetHeightMapChoice();
        int width = terrainData.heightmapWidth - 1;
        int squareSize = width;
        float heightMin = MPDHeightMin;
        float heightMax = MPDHeightMax;
        float heightDampener = (float)Mathf.Pow(MPDHeightDampenerPower, -1 * MPDRoughness);

        int cornerX, cornerY;
        int midX, midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;

        while (squareSize > 0.0f)
        {
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    heightMap[midX, midY] = (float)((heightMap[x, y] + heightMap[cornerX, y] + heightMap[x, cornerY] + heightMap[cornerX, cornerY]) / 4.0f + UnityEngine.Random.Range(heightMin, heightMax));
                }
            }

            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    pmidXR = (int)(midX + squareSize);
                    pmidYU = (int)(midY + squareSize);
                    pmidXL = (int)(midX - squareSize);
                    pmidYD = (int)(midY - squareSize);

                    // Make sure all points are within range
                    if (pmidXL <= 0 || pmidYD <= 0 || pmidXR >= width - 1 || pmidYU >= width - 1) continue;

                    // Calc the square val for the bottom side
                    heightMap[midX, y] = (float)((heightMap[midX, midY] + heightMap[x, y] + heightMap[midX, pmidYD] + heightMap[cornerX, y]) / 4.0f + UnityEngine.Random.Range(heightMin, heightMax));
                    // Calc the square val for the top side
                    heightMap[midX, cornerY] = (float)((heightMap[x, cornerY] + heightMap[midX, midY] + heightMap[cornerX, cornerY] + heightMap[midX, pmidYU]) / 4.0f + UnityEngine.Random.Range(heightMin, heightMax));
                    // Calc the square val for the right side
                    heightMap[cornerX, midY] = (float)((heightMap[midX, y] + heightMap[pmidXR, midY] + heightMap[cornerX, cornerY] + heightMap[midX, midY]) / 4.0f + UnityEngine.Random.Range(heightMin, heightMax));
                    // Calc the square val for the left side
                    heightMap[x, midY] = (float)((heightMap[midX, midY] + heightMap[x, cornerY] + heightMap[pmidXL, midY] + heightMap[x, y]) / 4.0f + UnityEngine.Random.Range(heightMin, heightMax));
                }
            }
            squareSize = (int)(squareSize / 2.0f);
            heightMin *= heightDampener;
            heightMax *= heightDampener;
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void Smooth()
    {
        float[,] heightMap = GetHeightMap();
        float smoothProgress = 0;
        EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress);

        for (int i = 0; i < smoothAmount; i++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                for (int x = 0; x < terrainData.heightmapWidth; x++)
                {
                    float avgHeight = heightMap[x, y];
                    List<Vector2> neighbours = GenerateNeighbours(new Vector2(x, y), terrainData.heightmapWidth, terrainData.heightmapHeight);

                    foreach (Vector2 n in neighbours)
                    {
                        avgHeight += heightMap[(int)n.x, (int)n.y];
                    }

                    heightMap[x, y] = avgHeight / ((float)neighbours.Count + 1);
                }
            }
            smoothProgress++;
            EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress / smoothAmount);
        }
        terrainData.SetHeights(0, 0, heightMap);
        EditorUtility.ClearProgressBar();
    }

    public void AddNewSplatHeight()
    {
        splatHeights.Add(new SplatHeights());
    }

    public void RemoveSplatHeight()
    {
        List<SplatHeights> keptSplatHeights = new List<SplatHeights>();

        for (int i = 0; i < splatHeights.Count; i++)
        {
            if (!splatHeights[i].remove) keptSplatHeights.Add(splatHeights[i]);
        }
        // Must keep atleast one thing in the array
        if (keptSplatHeights.Count == 0) keptSplatHeights.Add(splatHeights[0]);
        splatHeights = keptSplatHeights;
    }

    public void SplatMaps()
    {
        SplatPrototype[] newSplatProto;
        int spindex = 0;

        newSplatProto = new SplatPrototype[splatHeights.Count];
        foreach (SplatHeights sh in splatHeights)
        {
            newSplatProto[spindex] = new SplatPrototype();
            newSplatProto[spindex].texture = sh.texture;
            newSplatProto[spindex].tileOffset = sh.tileOffset;
            newSplatProto[spindex].tileSize = sh.tileSize;
            newSplatProto[spindex].texture.Apply(true);
            spindex++;
        }
        terrainData.splatPrototypes = newSplatProto;

        float[,] heightMap = GetHeightMap();
        float[,,] splatMapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float[] splat = new float[terrainData.alphamapLayers];

                for (int i = 0; i < splatHeights.Count; i++)
                {
                    float blendNoise = Mathf.PerlinNoise(x * splatHeights[i].splatNoiseVScale.x, y * splatHeights[i].splatNoiseVScale.y) * splatHeights[i].splatNoiseScaler;
                    float blendOffsetCalc = splatHeights[i].splatBlendOffset + blendNoise;
                    float thisHeightStart = splatHeights[i].minHeight - blendOffsetCalc;
                    float thisHeightStop = splatHeights[i].maxHeight + blendOffsetCalc;
                    // float steepness = GetSteepness(heightMap, x, y, terrainData.heightmapWidth, terrainData.heightmapHeight);
                    float steepness = terrainData.GetSteepness(y / (float)terrainData.alphamapHeight, x / (float)terrainData.alphamapWidth);

                    if ((heightMap[x, y] >= thisHeightStart && heightMap[x, y] <= thisHeightStop) && (steepness >= splatHeights[i].minSlope && steepness <= splatHeights[i].maxSlope))
                        splat[i] = 1;
                }
                NormalizeVector(splat);
                for (int j = 0; j < splatHeights.Count; j++) splatMapData[x, y, j] = splat[j];
            }
        }
        terrainData.SetAlphamaps(0, 0, splatMapData);
    }

    public void ResetTerrain()
    {
        float[,] heightMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];

        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                heightMap[x, y] = 0.0f;
            }
        }
        // Applying the height map to the terrain at position (0, 0)
        terrainData.SetHeights(0, 0, heightMap);
    }
}
