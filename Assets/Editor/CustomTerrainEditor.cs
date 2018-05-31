using UnityEngine;
using UnityEditor;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor {

    // Properties ----------
    // The height range used for the rand gen
    SerializedProperty randomHeightRange;

    // The height map scale used for gen
    SerializedProperty heightMapScale;
    // The height map image used for gen
    SerializedProperty heightMapImage;

    // The scaling from height map to perlin on x-axis
    SerializedProperty perlinXScale;
    // The scaling from height map to perlin on y-axis
    SerializedProperty perlinYScale;
    // The translation from height map to perlin on x-axis
    SerializedProperty perlinOffsetX;
    // The translation from height map to perlin on y-axis
    SerializedProperty perlinOffsetY;
    // The amount of detail in the perlin map
    SerializedProperty perlinOctaves;
    // The amount of consistency in the perlin map
    SerializedProperty perlinPersistance;
    // The height scale for the perlin map
    SerializedProperty perlinHeightScale;

    // The count for how many peak will be generated
    SerializedProperty voronoiPeaks;
    // The min height for each voronoi peak
    SerializedProperty voronoiMinHeight;
    // The max height for each voronoi peak
    SerializedProperty voronoiMaxHeight;
    // The steepness of the slope for each mountian
    SerializedProperty voronoiFallOff;
    // The curve of the slope for each mountian
    SerializedProperty voronoiDropOff;
    // If the voronoi type is Combined, Linear, Power
    SerializedProperty voronoiType;

    // The min height the MPD can produce
    SerializedProperty MPDHeightMin;
    // The max height the MPD can produce
    SerializedProperty MPDHeightMax;
    // The height dampener to reduce height over time
    SerializedProperty MPDHeightDampenerPower;
    SerializedProperty MPDRoughness;

    // The table for our perlin paramters to display the parameters
    GUITableState perlinParameterTable;
    // The perlin parameters list that we want to display
    SerializedProperty perlinParameters;

    // The table for our splat heights to display parameters
    GUITableState splatMapTable;
    // The splat heights list that we want to display
    SerializedProperty splatHeights;

    // The maximum amount of trees aloud on the terrain
    SerializedProperty vegMaxTrees;
    // How spaced out the trees will be
    SerializedProperty vegTreeSpacing;
    // The table for our vegetation to display parameters
    GUITableState vegetationTable;
    // The vegetation list that we want to display
    SerializedProperty vegetationList;

    // The maximum amount of details aloud on the terrain
    SerializedProperty maxDetails;
    // How spaced out the details will be
    SerializedProperty detailSpacing;
    // The table for our details to display parameters
    GUITableState detailTable;
    // The details list that we want to display
    SerializedProperty detailList;

    // The height that we want our water to be at from 0 to 1
    SerializedProperty waterHeight;
    // The game object we will use to place our water object
    SerializedProperty waterGO;
    // The material we will use for our shoreline animations
    SerializedProperty shoreLineMat;

    // The type of erosion we will use for our erosion method
    SerializedProperty erosionType;
    // The strength of erosion we will use for our erosion method
    SerializedProperty erosionStrength;
    // The strength of thermal erosion
    SerializedProperty erosionAmount;
    // The solubility of erosion we will use for our erosion method
    SerializedProperty erosionSolubility;
    // The amount of rain drops we will use for our erosion method
    SerializedProperty erosionDroplets;
    // The amount of river springs we will use for our erosion method
    SerializedProperty erosionsRiverSprings;
    // The amount of smoothing we will use for our erosion method
    SerializedProperty erosionSmoothAmount;

    // The amount of clouds we will use for cloud gen
    SerializedProperty numClouds;
    // The amount of particles we will use for cloud gen
    SerializedProperty particlesPerCloud;
    // The scale we will use for cloud gen
    SerializedProperty cloudScale;
    // The material we will use for cloud gen
    SerializedProperty cloudMat;
    // The material we will use for cloud shadow gen
    SerializedProperty cloudShadowMat;
    // The color we will use for cloud gen
    SerializedProperty cloudColor;
    // The secondary color we will use for cloud gen
    SerializedProperty cloudLining;
    // The start size we will use for cloud gen
    SerializedProperty cloudStartSize;
    // The min speed we will use for cloud movement
    SerializedProperty cloudMinSpeed;
    // The max speed we will use for cloud movement
    SerializedProperty cloudMaxSpeed;
    // The range we will use for cloud movement
    SerializedProperty cloudRange;

    // How many loops the smooth algo does
    SerializedProperty smoothAmount;

    // The boolean to see if the terrain should reset before generating
    SerializedProperty resetBeforeGen;

    // Fold outs ------------
    // Fold out for the random hieght generation properties
    bool showRandom = false;
    // Fold out for the image import for heights
    bool showLoadHeights = false;
    // Fold out for the perlin noise generation
    bool showPerlin = false;
    // Fold out for the multiple perlin noise generator
    bool showMultiPerlin = false;
    // Fold out for the voronoi noise generator
    bool showVoronoi = false;
    // Fold out for midpoint displacement algo
    bool showMPD = false;
    // Fold out for smoothing algo
    bool showSmooth = false;
    // Fold out for splat map generator
    bool showSplatMap = false;
    // Foldout for vegetation generator
    bool showVegetation = false;
    // Foldout for detail generator
    bool showDetail = false;
    // Foldout for water generator
    bool showWater = false;
    // Foldout for erosion
    bool showErosion = false;
    // Foldout for clouds
    bool showClouds = false;
    // Fold out for height map display
    bool showHeightMap = false;

    // Displayed -----------
    Texture2D heightMapTexture;

    // Scrollbar global
    Vector2 scrollPos;

    private void RefreshHeightMapDisplay(CustomTerrain terrain)
    {
        float[,] heightMap = terrain.GetHeightMap();

        for (int y = 0; y < terrain.terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrain.terrainData.heightmapWidth; x++)
            {
                heightMapTexture.SetPixel(x, y, new Color(heightMap[x, y], heightMap[x, y], heightMap[x, y], 1.0f));
            }
        }
        heightMapTexture.Apply();
    }

    // To allow us to recompile in editor without playing
    void OnEnable()
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");

        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapImage = serializedObject.FindProperty("heightMapImage");

        perlinXScale = serializedObject.FindProperty("perlinXScale");
        perlinYScale = serializedObject.FindProperty("perlinYScale");
        perlinOffsetX = serializedObject.FindProperty("perlinOffsetX");
        perlinOffsetY = serializedObject.FindProperty("perlinOffsetY");
        perlinOctaves = serializedObject.FindProperty("perlinOctaves");
        perlinPersistance = serializedObject.FindProperty("perlinPersistance");
        perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");

        voronoiPeaks = serializedObject.FindProperty("voronoiPeaks");
        voronoiMinHeight = serializedObject.FindProperty("voronoiMinHeight");
        voronoiMaxHeight = serializedObject.FindProperty("voronoiMaxHeight");
        voronoiFallOff = serializedObject.FindProperty("voronoiFallOff");
        voronoiDropOff = serializedObject.FindProperty("voronoiDropOff");
        voronoiType = serializedObject.FindProperty("voronoiType");

        MPDHeightMin = serializedObject.FindProperty("MPDHeightMin");
        MPDHeightMax = serializedObject.FindProperty("MPDHeightMax");
        MPDHeightDampenerPower = serializedObject.FindProperty("MPDHeightDampenerPower");
        MPDRoughness = serializedObject.FindProperty("MPDRoughness");

        perlinParameterTable = new GUITableState("perlinParameterTable");
        perlinParameters = serializedObject.FindProperty("perlinParameters");

        splatMapTable = new GUITableState("splatMapTable");
        splatHeights = serializedObject.FindProperty("splatHeights");

        vegMaxTrees = serializedObject.FindProperty("vegMaxTrees");
        vegTreeSpacing = serializedObject.FindProperty("vegTreeSpacing");
        vegetationTable = new GUITableState("vegetationTable");
        vegetationList = serializedObject.FindProperty("vegetationList");

        maxDetails = serializedObject.FindProperty("maxDetails");
        detailSpacing = serializedObject.FindProperty("detailSpacing");
        detailTable = new GUITableState("detailTable");
        detailList = serializedObject.FindProperty("detailList");

        waterHeight = serializedObject.FindProperty("waterHeight");
        waterGO = serializedObject.FindProperty("waterGO");
        shoreLineMat = serializedObject.FindProperty("shoreLineMat");

        erosionType = serializedObject.FindProperty("erosionType");
        erosionStrength = serializedObject.FindProperty("erosionStrength");
        erosionAmount = serializedObject.FindProperty("erosionAmount");
        erosionSolubility = serializedObject.FindProperty("erosionSolubility");
        erosionDroplets = serializedObject.FindProperty("erosionDroplets");
        erosionsRiverSprings = serializedObject.FindProperty("erosionsRiverSprings");
        erosionSmoothAmount = serializedObject.FindProperty("erosionSmoothAmount");

        numClouds = serializedObject.FindProperty("numClouds");
        particlesPerCloud = serializedObject.FindProperty("particlesPerCloud");
        cloudScale = serializedObject.FindProperty("cloudScale");
        cloudMat = serializedObject.FindProperty("cloudMat");
        cloudShadowMat = serializedObject.FindProperty("cloudShadowMat");
        cloudColor = serializedObject.FindProperty("cloudColor");
        cloudLining = serializedObject.FindProperty("cloudLining");
        cloudStartSize = serializedObject.FindProperty("cloudStartSize");
        cloudMinSpeed = serializedObject.FindProperty("cloudMinSpeed");
        cloudMaxSpeed = serializedObject.FindProperty("cloudMaxSpeed");
        cloudRange = serializedObject.FindProperty("cloudRange");

        smoothAmount = serializedObject.FindProperty("smoothAmount");

        resetBeforeGen = serializedObject.FindProperty("resetBeforeGen");

        heightMapTexture = new Texture2D(513, 513, TextureFormat.ARGB32, false);
    }

    public override void OnInspectorGUI()
    {
        int wSize = (int)(EditorGUIUtility.currentViewWidth - 100);

        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;

        // Scrollbar starting code
        Rect r = EditorGUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(r.width), GUILayout.Height(r.height));
        EditorGUI.indentLevel++;

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        showRandom = EditorGUILayout.Foldout(showRandom, "RandomGenProps");
        if (showRandom)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Height Randomly Between Two Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange);
            if (GUILayout.Button("Gen Random Heights")) terrain.RandomTerrain();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights, "ImageGenProps");
        if (showLoadHeights)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Load Height From Texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightMapImage);
            EditorGUILayout.PropertyField(heightMapScale);
            if (GUILayout.Button("Load Texture Heights")) terrain.LoadTexture();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        showPerlin = EditorGUILayout.Foldout(showPerlin, "PerlinGenProps");
        if (showPerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Height Based On Perlin Noise", EditorStyles.boldLabel);
            EditorGUILayout.Slider(perlinXScale, 0.0f, 1.0f, new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinYScale, 0.0f, 1.0f, new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(perlinOffsetX, 0, 10000, new GUIContent("X Offset"));
            EditorGUILayout.IntSlider(perlinOffsetY, 0, 10000, new GUIContent("Y Offset"));
            EditorGUILayout.IntSlider(perlinOctaves, 1, 10, new GUIContent("Octaves"));
            EditorGUILayout.Slider(perlinPersistance, 0.1f, 10.0f, new GUIContent("Persistance"));
            EditorGUILayout.Slider(perlinHeightScale, 0.0f, 1.0f, new GUIContent("Perlin Height Scale"));
            if (GUILayout.Button("Gen Perlin Heights")) terrain.Perlin();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        showMultiPerlin = EditorGUILayout.Foldout(showMultiPerlin, "MultiPerlinGenProps");
        if (showMultiPerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Height Based On Multiple Perlin Noise", EditorStyles.boldLabel);
            perlinParameterTable = GUITableLayout.DrawTable(perlinParameterTable, serializedObject.FindProperty("perlinParameters"));

            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+")) terrain.AddNewPerlin();
            if (GUILayout.Button("-")) terrain.RemovePerlin();
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Gen Multi Perlin Heights")) terrain.MultiplePerlinTerrain();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        showVoronoi = EditorGUILayout.Foldout(showVoronoi, "VoronoiGenProps");
        if (showVoronoi)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Height Based On Voronoi Noise", EditorStyles.boldLabel);

            EditorGUILayout.IntSlider(voronoiPeaks, 1, 10, new GUIContent("Peaks"));
            EditorGUILayout.Slider(voronoiMinHeight, 0.0f, 1.0f, new GUIContent("Min Height"));
            EditorGUILayout.Slider(voronoiMaxHeight, 0.0f, 1.0f, new GUIContent("Max Height"));
            EditorGUILayout.Slider(voronoiFallOff, 0.0f, 10.0f, new GUIContent("Fall Off"));
            EditorGUILayout.Slider(voronoiDropOff, 0.0f, 10.0f, new GUIContent("Drop Off"));
            EditorGUILayout.PropertyField(voronoiType);
            if (GUILayout.Button("Gen Voronoi Heights")) terrain.Voronoi();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        showMPD = EditorGUILayout.Foldout(showMPD, "MPDGenProps");
        if (showMPD)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Height Base On MPD ", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(MPDHeightMin);
            EditorGUILayout.PropertyField(MPDHeightMax);
            EditorGUILayout.PropertyField(MPDHeightDampenerPower);
            EditorGUILayout.PropertyField(MPDRoughness);
            if (GUILayout.Button("MPD")) terrain.MPD();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        showSplatMap = EditorGUILayout.Foldout(showSplatMap, "SplatMapProps");
        if (showSplatMap)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Texture The Terrain With Splat Map", EditorStyles.boldLabel);
            splatMapTable = GUITableLayout.DrawTable(splatMapTable, serializedObject.FindProperty("splatHeights"));

            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+")) terrain.AddNewSplatHeight();
            if (GUILayout.Button("-")) terrain.RemoveSplatHeight();
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Apply SplatMap")) terrain.SplatMaps();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        showVegetation = EditorGUILayout.Foldout(showVegetation, "VegGenProps");
        if (showVegetation)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Generate Vegetation", EditorStyles.boldLabel);
            EditorGUILayout.IntSlider(vegMaxTrees, 0, 10000, new GUIContent("Max Trees"));
            EditorGUILayout.IntSlider(vegTreeSpacing, 2, 20, new GUIContent("Tree Spacing"));
            vegetationTable = GUITableLayout.DrawTable(vegetationTable, serializedObject.FindProperty("vegetationList"));

            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+")) terrain.AddNewVegetation();
            if (GUILayout.Button("-")) terrain.RemoveVegetation();
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Generate Vegetation")) terrain.PlaceVegetation();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        showDetail = EditorGUILayout.Foldout(showDetail, "DetailGenProps");
        if (showDetail)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Generate Detail", EditorStyles.boldLabel);
            EditorGUILayout.IntSlider(maxDetails, 0, 10000, new GUIContent("Max Detail"));
            terrain.GetComponent<Terrain>().detailObjectDistance = maxDetails.intValue;
            EditorGUILayout.IntSlider(detailSpacing, 2, 20, new GUIContent("Detail Spacing"));
            detailTable = GUITableLayout.DrawTable(detailTable, serializedObject.FindProperty("detailList"));

            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+")) terrain.AddNewDetails();
            if (GUILayout.Button("-")) terrain.RemoveDetails();
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Generate Details")) terrain.PlaceDetails();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        showWater = EditorGUILayout.Foldout(showWater, "WaterGenProps");
        if (showWater)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Generate Water", EditorStyles.boldLabel);

            EditorGUILayout.Slider(waterHeight, 0.0f, 1.0f, new GUIContent("Water Height"));
            EditorGUILayout.PropertyField(waterGO);
            if (GUILayout.Button("Add Water")) terrain.AddWater();

            EditorGUILayout.PropertyField(shoreLineMat);
            if (GUILayout.Button("Add Shore")) terrain.DrawShoreLine();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        showErosion = EditorGUILayout.Foldout(showErosion, "ErosionProps");
        if (showErosion)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Erode Landscape", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(erosionType);
            EditorGUILayout.Slider(erosionStrength, 0.0f, 1.0f, new GUIContent("Erosion Strength"));
            EditorGUILayout.Slider(erosionAmount, 0.0f, 1.0f, new GUIContent("Erosion Amount"));
            EditorGUILayout.Slider(erosionSolubility, 0.001f, 1.0f, new GUIContent("Solubility"));
            EditorGUILayout.IntSlider(erosionDroplets, 0, 500, new GUIContent("Droplets"));
            EditorGUILayout.IntSlider(erosionsRiverSprings, 0, 20, new GUIContent("Springs Per River"));
            EditorGUILayout.IntSlider(erosionSmoothAmount, 0, 10, new GUIContent("Smooth Amount"));

            if (GUILayout.Button("Erode")) terrain.Erode();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        showClouds = EditorGUILayout.Foldout(showClouds, "CloudProps");
        if (showClouds)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Generate Clouds In The Sky", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(numClouds, new GUIContent("Number of Clouds"));
            EditorGUILayout.PropertyField(particlesPerCloud, new GUIContent("Particles Per Cloud"));
            EditorGUILayout.PropertyField(cloudScale, new GUIContent("Size"));
            EditorGUILayout.PropertyField(cloudMat, true);
            EditorGUILayout.PropertyField(cloudShadowMat, true);
            EditorGUILayout.PropertyField(cloudColor, new GUIContent("Color"));
            EditorGUILayout.PropertyField(cloudLining, new GUIContent("Lining"));
            EditorGUILayout.PropertyField(cloudStartSize, new GUIContent("Cloud Particle Size"));
            EditorGUILayout.PropertyField(cloudMinSpeed, new GUIContent("Min Speed"));
            EditorGUILayout.PropertyField(cloudMaxSpeed, new GUIContent("Max Speed"));
            EditorGUILayout.PropertyField(cloudRange, new GUIContent("Dst Travelled"));

            if (GUILayout.Button("Generate Clouds")) terrain.GenerateClouds();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        showSmooth = EditorGUILayout.Foldout(showSmooth, "SmoothProps");
        if (showSmooth)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Smooth The Terrain", EditorStyles.boldLabel);
            EditorGUILayout.IntSlider(smoothAmount, 1, 10, new GUIContent("Smooth Amount"));
            if (GUILayout.Button("Smooth")) terrain.SmoothTerrain();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.PropertyField(resetBeforeGen);
        if (GUILayout.Button("Reset Terrain")) terrain.ResetTerrain();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        showHeightMap = EditorGUILayout.Foldout(showHeightMap, "ShowHeightMap");
        if (showHeightMap)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("View The Height Map", EditorStyles.boldLabel, GUILayout.Width(wSize / 2));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(heightMapTexture, GUILayout.Width(wSize), GUILayout.Height(wSize));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Refresh", GUILayout.Width(wSize))) RefreshHeightMapDisplay(terrain);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        //Scrollbar ending code
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
