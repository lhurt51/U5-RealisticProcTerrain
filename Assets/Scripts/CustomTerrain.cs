using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour {
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

    // Acess to terrain data ---------------------------------------
    public Terrain terrain;
    public TerrainData terrainData;

    // Should it reset the terrain before generating a new height map
    public bool resetBeforeGen;

    float[,] GetHeightMap()
    {
        if (!resetBeforeGen) return terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        else return new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
    }

    public void Perlin()
    {
        float[,] heightMap = GetHeightMap();

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

    public void MultiplePerlinTerrain()
    {
        float[,] heightMap = GetHeightMap();

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

    public void RandomTerrain()
    {
        float[,] heightMap = GetHeightMap();

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
        float[,] heightMap = GetHeightMap();

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

    public void Voronoi()
    {
        float[,] heightMap = GetHeightMap();

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

    void AddTag(SerializedProperty tagsProp, string newTag)
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

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
