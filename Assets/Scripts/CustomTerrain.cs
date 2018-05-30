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
    public enum VoronoiType
    {
        Linear = 0,
        Power = 1,
        Combined = 2,
        SinPow = 3
    }

    public int voronoiPeaks = 5;
    public float voronoiMinHeight = 0.0f;
    public float voronoiMaxHeight = 1.0f;
    public float voronoiFallOff = 0.2f;
    public float voronoiDropOff = 0.6f;
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

    // Vegetation --------------------------------------------------
    public int vegMaxTrees = 5000;
    public int vegTreeSpacing = 5;

    [System.Serializable]
    public class Vegetation
    {
        public GameObject mesh;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0.0f;
        public float maxSlope = 70.0f;
        public float minScale = 0.5f;
        public float maxScale = 1.2f;
        public float minRotation = 0.0f;
        public float maxRotation = 360.0f;
        public float density = 0.5f;
        public Color color1 = Color.white;
        public Color color2 = Color.white;
        public Color lightColor = Color.white;
        public bool remove = false;
    }

    public List<Vegetation> vegetationList = new List<Vegetation>()
    {
        new Vegetation()
    };

    // Details -----------------------------------------------------
    public int maxDetails = 5000;
    public int detailSpacing = 5;

    [System.Serializable]
    public class Detail
    {
        public GameObject prototype = null;
        public Texture2D prototypeTexture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0.0f;
        public float maxSlope = 70.0f;
        public float noiseSpread = 0.5f;
        public float overlap = 0.01f;
        public float feather = 0.05f;
        public float density = 0.5f;
        public Color dryColor = Color.white;
        public Color healthyColor = Color.white;
        public Vector2 heightRange = new Vector2(1.0f, 1.0f);
        public Vector2 widthRange = new Vector2(1.0f, 1.0f);
        public bool remove = false;
    }

    public List<Detail> detailList = new List<Detail>()
    {
        new Detail()
    };

    // Water -------------------------------------------------------
    public float waterHeight = 0.5f;
    public GameObject waterGO;
    public Material shoreLineMat;

    // Erosion -----------------------------------------------------
    public enum ErosionType
    {
        Rain = 0,
        Thermal = 1,
        Tidal = 2,
        River = 3,
        Wind = 4,
    }

    public ErosionType erosionType = ErosionType.Rain;
    public float erosionStrength = 0.1f;
    public float erosionAmount = 0.01f;
    public float erosionSolubility = 0.01f;
    public int erosionDroplets = 10;
    public int erosionsRiverSprings = 5;
    public int erosionSmoothAmount = 5;

    // Smooth Algo -------------------------------------------------
    public int smoothAmount = 5;

    // Should it reset the terrain before generating a new height map
    public bool resetBeforeGen;

    // Terrain Settings For Raycasting -----------------------------
    [SerializeField]
    int terrainLayer = -1;

    public enum TagType
    {
        Tag = 0,
        Layer = 1
    }

    // Private Class Methods ----------------------------------------
    private int AddTag(SerializedProperty tagsProp, string newTag, TagType tagType)
    {
        bool found = false;

        // Ensure the tag doesn't already exist
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newTag)) { found = true; return i; }
        }

        // Add new tag
        if (!found && tagType == TagType.Tag)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }
        // Add new layer
        else if (!found && tagType == TagType.Layer)
        {
            for (int j = 8; j < tagsProp.arraySize; j++)
            {
                SerializedProperty newLayer = tagsProp.GetArrayElementAtIndex(j);

                // Add layer in next empty slot
                if (newLayer.stringValue == "")
                {
                    newLayer.stringValue = newTag;
                    return j;
                }
            }
        }
        return -1;
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
        SerializedProperty layerProp = tagManager.FindProperty("layers");

        AddTag(tagsProp, "Terrain", TagType.Tag);
        AddTag(tagsProp, "Cloud", TagType.Tag);
        AddTag(tagsProp, "Shore", TagType.Tag);

        terrainLayer = AddTag(layerProp, "Terrain", TagType.Layer);

        // Apply tag changes to the tag database
        tagManager.ApplyModifiedProperties();

        // Tag the game object as terrain
        this.gameObject.tag = "Terrain";
        // Set the collision layer to terrain
        this.gameObject.layer = terrainLayer;
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

    private void Smooth(int thisSmoothAmount)
    {
        float[,] heightMap = GetHeightMap();
        float smoothProgress = 0;
        EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress);

        for (int i = 0; i < thisSmoothAmount; i++)
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
            EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress / thisSmoothAmount);
        }
        terrainData.SetHeights(0, 0, heightMap);
        EditorUtility.ClearProgressBar();
    }

    private float[,] RunRiver(Vector3 dropletPos, float[,] heightMap, float[,] erosionMap, int width, int height)
    {
        while (erosionMap[(int)dropletPos.x, (int)dropletPos.y] > 0)
        {
            List<Vector2> neighbours = GenerateNeighbours(dropletPos, width, height);
            bool foundLower = false;

            neighbours.Shuffle();
            foreach (Vector2 n in neighbours)
            {
                if (heightMap[(int)n.x, (int)n.y] < heightMap[(int)dropletPos.x, (int)dropletPos.y])
                {
                    erosionMap[(int)n.x, (int)n.y] = erosionMap[(int)dropletPos.x, (int)dropletPos.y] - erosionSolubility;
                    dropletPos = n;
                    foundLower = true;
                    break;
                }
            }
            if (!foundLower) erosionMap[(int)dropletPos.x, (int)dropletPos.y] -= erosionSolubility;
        }
        return erosionMap;
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
            if (!perlinParameters[i].remove) keptPerlinParameters.Add(perlinParameters[i]);
        }

        // If there is nothing in the list. Add a new perlin paramater back to the list
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

    public void SmoothTerrain()
    {
        Smooth(smoothAmount);
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

    public void AddNewVegetation()
    {
        vegetationList.Add(new Vegetation());
    }

    public void RemoveVegetation()
    {
        List<Vegetation> keptVegetation = new List<Vegetation>();

        for (int i = 0; i < vegetationList.Count; i++)
        {
            if (!vegetationList[i].remove) keptVegetation.Add(vegetationList[i]);
        }
        // Must keep atleast one thing in the array
        if (keptVegetation.Count == 0) keptVegetation.Add(vegetationList[0]);
        vegetationList = keptVegetation;
    }

    public void PlaceVegetation()
    {
        int tindex = 0;
        TreePrototype[] newTreeProtos;
        List<TreeInstance> allVeg = new List<TreeInstance>();

        newTreeProtos = new TreePrototype[vegetationList.Count];
        foreach (Vegetation t in vegetationList)
        {
            newTreeProtos[tindex] = new TreePrototype();
            newTreeProtos[tindex].prefab = t.mesh;
            tindex++;
        }
        terrainData.treePrototypes = newTreeProtos;

        for (int z = 0; z < terrainData.size.z; z += vegTreeSpacing)
        {
            for (int x = 0; x < terrainData.size.x; x += vegTreeSpacing)
            {
                for (int tp = 0; tp < terrainData.treePrototypes.Length; tp++)
                {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) > vegetationList[tp].density) break;

                    float thisHeight = terrainData.GetHeight(x, z) / terrainData.size.y;
                    float thisHeightStart = vegetationList[tp].minHeight;
                    float thisHeightEnd = vegetationList[tp].maxHeight;
                    float steepness = terrainData.GetSteepness(x / (float)terrainData.size.x, z / (float)terrainData.size.z);

                    if ((thisHeight >= thisHeightStart && thisHeight <= thisHeightEnd) && (steepness >= vegetationList[tp].minSlope && steepness <= vegetationList[tp].maxSlope))
                    {
                        TreeInstance instance = new TreeInstance();
                        instance.position = new Vector3((x + UnityEngine.Random.Range(-5.0f, 5.0f)) / terrainData.size.x, thisHeight, (z + UnityEngine.Random.Range(-5.0f, 5.0f)) / terrainData.size.z);

                        // Reposition the trees against the ground
                        Vector3 treeWorldPos = new Vector3(instance.position.x * terrainData.size.x, instance.position.y * terrainData.size.y, instance.position.z * terrainData.size.z) + this.transform.position;
                        RaycastHit hit;
                        int layerMask = 1 << terrainLayer;

                        if (Physics.Raycast(treeWorldPos + new Vector3(0.0f, 10.0f, 0.0f), -Vector2.up, out hit, 100, layerMask) || Physics.Raycast(treeWorldPos + new Vector3(0.0f, -10.0f, 0.0f), Vector2.up, out hit, 100, layerMask))
                        {
                            float scale = UnityEngine.Random.Range(vegetationList[tp].minScale, vegetationList[tp].maxScale);
                            float treeHeight = (hit.point.y - this.transform.position.y) / terrainData.size.y;

                            // Setting all properties
                            instance.position = new Vector3(instance.position.x, treeHeight, instance.position.z);
                            instance.rotation = UnityEngine.Random.Range(vegetationList[tp].minRotation, vegetationList[tp].maxRotation);
                            instance.prototypeIndex = tp;
                            instance.color = Color.Lerp(vegetationList[tp].color1, vegetationList[tp].color2, UnityEngine.Random.Range(0.0f, 1.0f));
                            instance.lightmapColor = vegetationList[tp].lightColor;
                            instance.heightScale = scale;
                            instance.widthScale = scale;

                            // Adding it to our terrain
                            allVeg.Add(instance);
                            if (allVeg.Count >= vegMaxTrees) goto TREESDONE;
                        }
                    }
                }
            }
        }
        TREESDONE:
            terrainData.treeInstances = allVeg.ToArray();
    }

    public void AddNewDetails()
    {
        detailList.Add(new Detail());
    }

    public void RemoveDetails()
    {
        List<Detail> keptDetails = new List<Detail>();

        for (int i = 0; i < detailList.Count; i++)
        {
            if (!detailList[i].remove) keptDetails.Add(detailList[i]);
        }
        // If there is nothing in the list. Add a new detail back to the list
        if (keptDetails.Count == 0) keptDetails.Add(detailList[0]);
        detailList = keptDetails;
    }

    public void PlaceDetails()
    {
        int dindex = 0;
        DetailPrototype[] newDetailPrototypes;

        newDetailPrototypes = new DetailPrototype[detailList.Count];
        foreach(Detail d in detailList)
        {
            newDetailPrototypes[dindex] = new DetailPrototype();
            newDetailPrototypes[dindex].prototype = d.prototype;
            newDetailPrototypes[dindex].prototypeTexture = d.prototypeTexture;
            newDetailPrototypes[dindex].healthyColor = d.healthyColor;
            newDetailPrototypes[dindex].dryColor = d.dryColor;
            newDetailPrototypes[dindex].minHeight = d.heightRange.x;
            newDetailPrototypes[dindex].maxHeight = d.heightRange.y;
            newDetailPrototypes[dindex].minWidth = d.widthRange.x;
            newDetailPrototypes[dindex].maxWidth = d.widthRange.y;
            newDetailPrototypes[dindex].noiseSpread = d.noiseSpread;
            if (newDetailPrototypes[dindex].prototype)
            {
                newDetailPrototypes[dindex].usePrototypeMesh = true;
                newDetailPrototypes[dindex].renderMode = DetailRenderMode.VertexLit;
            }
            else
            {
                newDetailPrototypes[dindex].usePrototypeMesh = false;
                newDetailPrototypes[dindex].renderMode = DetailRenderMode.GrassBillboard;
            }
            dindex++;
        }
        terrainData.detailPrototypes = newDetailPrototypes;

        float[,] heightMap = GetHeightMap();

        for (int i = 0; i < terrainData.detailPrototypes.Length; i++)
        {
            int[,] detailMap = new int[terrainData.detailWidth, terrainData.detailHeight];

            for (int y = 0; y < terrainData.detailHeight; y += detailSpacing)
            {
                for (int x = 0; x < terrainData.detailWidth; x += detailSpacing)
                {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) > detailList[i].density) continue;

                    int xHM = (int)(x / (float)terrainData.detailWidth * terrainData.heightmapWidth);
                    int yHM = (int)(y / (float)terrainData.detailHeight * terrainData.heightmapHeight);

                    float thisNoise = Utils.Map(Mathf.PerlinNoise(x * detailList[i].feather, y * detailList[i].feather), 0, 1, 0.5f, 1);
                    float thisHeightStart = detailList[i].minHeight * thisNoise - detailList[i].overlap * thisNoise;
                    float nextHeightStart = detailList[i].maxHeight * thisNoise + detailList[i].overlap * thisNoise;
                    float thisHeight = heightMap[yHM, xHM];
                    float steepness = terrainData.GetSteepness(xHM / (float)terrainData.size.x, yHM / (float)terrainData.size.z);
                    if ((thisHeight >= thisHeightStart && thisHeight <= nextHeightStart) && (steepness >= detailList[i].minSlope && steepness <= detailList[i].maxSlope)) detailMap[y, x] = 1;
                }
            }
            terrainData.SetDetailLayer(0, 0, i, detailMap);
        }
    }

    public void AddWater()
    {
        GameObject water = GameObject.Find("water");

        if (!water)
        {
            water = Instantiate(waterGO, this.transform.position, this.transform.rotation);
            water.name = "water";
        }
        water.transform.position = this.transform.position + new Vector3(terrainData.size.x / 2.0f, waterHeight * terrainData.size.y, terrainData.size.z / 2.0f);
        water.transform.localScale = new Vector3(terrainData.size.x, 1.0f, terrainData.size.z);
    }

    public void DrawShoreLine()
    {
        float[,] heightMap = GetHeightMap();

        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                // Find a spot on the shore
                Vector2 thisLoc = new Vector2(x, y);
                List<Vector2> neighbours = GenerateNeighbours(thisLoc, terrainData.heightmapWidth, terrainData.heightmapHeight);

                foreach (Vector2 n in neighbours)
                {
                    if (heightMap[x, y] < waterHeight && heightMap[(int)n.x, (int)n.y] > waterHeight)
                    {
                        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        go.transform.position = this.transform.position + new Vector3(y / (float)terrainData.heightmapHeight * terrainData.size.z, waterHeight * terrainData.size.y, x / (float)terrainData.heightmapWidth * terrainData.size.x);
                        go.transform.LookAt(new Vector3(n.y / (float)terrainData.heightmapHeight * terrainData.size.z, waterHeight * terrainData.size.y, n.x / (float)terrainData.heightmapWidth * terrainData.size.x));
                        go.transform.Rotate(90.0f, 0.0f, 0.0f);
                        go.transform.localScale *= 25.0f;
                        go.tag = "Shore";
                    }
                }
            }
        }

        GameObject[] shoreQuads = GameObject.FindGameObjectsWithTag("Shore");
        MeshFilter[] meshFilters = new MeshFilter[shoreQuads.Length];

        for (int m = 0; m < shoreQuads.Length; m++)
        {
            meshFilters[m] = shoreQuads[m].GetComponent<MeshFilter>();
        }

        int i = 0;
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.active = false;
            i++;
        }

        GameObject curShoreLine = GameObject.Find("ShoreLine");

        if (curShoreLine) DestroyImmediate(curShoreLine);

        GameObject shoreLine = new GameObject();
        shoreLine.name = "ShoreLine";
        shoreLine.AddComponent<WaveAnimation>();
        shoreLine.transform.position = this.transform.position;
        shoreLine.transform.rotation = this.transform.rotation;

        MeshFilter thisMF = shoreLine.AddComponent<MeshFilter>();
        thisMF.mesh = new Mesh();
        shoreLine.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);

        MeshRenderer r = shoreLine.AddComponent<MeshRenderer>();
        r.sharedMaterial = shoreLineMat;

        for (int sQ = 0; sQ < shoreQuads.Length; sQ++) DestroyImmediate(shoreQuads[sQ]);
    }

    public void Rain()
    {
        float[,] heightMap = GetHeightMap();

        for (int i = 0; i < erosionDroplets; i++)
        {
            heightMap[UnityEngine.Random.Range(0, terrainData.heightmapWidth), UnityEngine.Random.Range(0, terrainData.heightmapHeight)] -= erosionStrength;
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void Tidal()
    {
        float[,] heightMap = GetHeightMap();

        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                Vector2 thisLoc = new Vector2(x, y);
                List<Vector2> neighbours = GenerateNeighbours(thisLoc, terrainData.heightmapWidth, terrainData.heightmapHeight);

                foreach (Vector2 n in neighbours)
                {
                    if (heightMap[x, y] < waterHeight && heightMap[(int)n.x, (int)n.y] > waterHeight)
                    {
                        heightMap[x, y] = waterHeight;
                        heightMap[(int)n.x, (int)n.y] = waterHeight;
                    }
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void Thermal()
    {
        float[,] heightMap = GetHeightMap();

        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                Vector2 thisLoc = new Vector2(x, y);
                List<Vector2> neighbours = GenerateNeighbours(thisLoc, terrainData.heightmapWidth, terrainData.heightmapHeight);

                foreach (Vector2 n in neighbours)
                {
                    if (heightMap[x, y] > heightMap[(int)n.x, (int)n.y] + erosionStrength)
                    {
                        float curHeight = heightMap[x, y];

                        heightMap[x, y] -= curHeight * erosionAmount;
                        heightMap[(int)n.x, (int)n.y] += curHeight * erosionAmount;
                    }
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void River()
    {
        float[,] heightMap = GetHeightMap();
        float[,] erosionMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];

        for (int i = 0; i < erosionDroplets; i++)
        {
            Vector2 dropletPos = new Vector2(UnityEngine.Random.Range(0, terrainData.heightmapWidth), UnityEngine.Random.Range(0, terrainData.heightmapHeight));

            erosionMap[(int)dropletPos.x, (int)dropletPos.y] = erosionStrength;
            for (int j = 0; j < erosionsRiverSprings; j++)
            {
                erosionMap = RunRiver(dropletPos, heightMap, erosionMap, terrainData.heightmapWidth, terrainData.heightmapHeight);
            }
        }

        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                if (erosionMap[x, y] > 0.0f) heightMap[x, y] -= erosionMap[x, y];
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void Wind()
    {
        float[,] heightMap = GetHeightMap();
        int width = terrainData.heightmapWidth;
        int height = terrainData.heightmapHeight;

        for (int y = 0; y <= height; y += 10)
        {
            for (int x = 0; x <= width; x += 1)
            {
                float thisNoise = (float)Mathf.PerlinNoise(x * 0.06f, y * 0.06f) * 20 * erosionStrength;
                int nx = (int)x;
                int ny = (int)y + 5 + (int)thisNoise;
                int digy = (int)y + (int)thisNoise;

                if (!(nx < 0 || nx > (width - 1) || ny < 0 || ny> (height - 1)))
                {
                    heightMap[x, digy] -= 0.001f;
                    heightMap[nx, ny] += 0.001f;
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void Erode()
    {
        if (erosionType == ErosionType.Rain)
            Rain();
        else if (erosionType == ErosionType.Tidal)
            Tidal();
        else if (erosionType == ErosionType.Thermal)
            Thermal();
        else if (erosionType == ErosionType.River)
            River();
        else if (erosionType == ErosionType.Wind)
            Wind();

        Smooth(erosionSmoothAmount);
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
