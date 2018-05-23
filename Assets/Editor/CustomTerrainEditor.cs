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
    // The boolean to see if the terrain should reset before generating
    SerializedProperty resetBeforeGen;
    // The table for our perlin paramters to display the parameters
    GUITableState perlinParameterTable;
    // The perlin parameters list that we want to display
    SerializedProperty perlinParameters;

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
        resetBeforeGen = serializedObject.FindProperty("resetBeforeGen");
        perlinParameterTable = new GUITableState("perlinParameterTable");
        perlinParameters = serializedObject.FindProperty("perlinParameters");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;

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

            if (GUILayout.Button("Gen Voronoi Heights")) terrain.Voronoi();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.PropertyField(resetBeforeGen);
        if (GUILayout.Button("Reset Terrain")) terrain.ResetTerrain();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        serializedObject.ApplyModifiedProperties();
    }
}
