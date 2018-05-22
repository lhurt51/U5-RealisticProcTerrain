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
    // The boolean to see if the terrain should reset before generating
    SerializedProperty resetBeforeGen;

    // Fold outs ------------
    // Fold out for the random hieght generation properties
    bool showRandom = false;
    // Fold out for the image import for heights
    bool showLoadHeights = false;
    // Fold out for the perlin noise generation
    bool showPerlin = false;

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
        resetBeforeGen = serializedObject.FindProperty("resetBeforeGen");
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
            GUILayout.Label("Set Terrain Height Randomly Between Two Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange);
            if (GUILayout.Button("Gen Random Heights"))
            {
                if (resetBeforeGen.boolValue) terrain.ResetTerrain();
                terrain.RandomTerrain();
            }
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights, "ImageGenProps");
        if (showLoadHeights)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Load Terrain Height From Texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightMapImage);
            EditorGUILayout.PropertyField(heightMapScale);
            if (GUILayout.Button("Load Texture Heights")) terrain.LoadTexture();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        showPerlin = EditorGUILayout.Foldout(showPerlin, "PerlinGenProps");
        if (showPerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Terrain Height Based On Perlin Noise", EditorStyles.boldLabel);
            EditorGUILayout.Slider(perlinXScale, 0.0f, 1.0f, new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinYScale, 0.0f, 1.0f, new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(perlinOffsetX, 0, 10000, new GUIContent("X Offset"));
            EditorGUILayout.IntSlider(perlinOffsetY, 0, 10000, new GUIContent("Y Offset"));
            if (GUILayout.Button("Gen Perlin Heights"))
            {
                if (resetBeforeGen.boolValue) terrain.ResetTerrain();
                terrain.Perlin();
            }
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.PropertyField(resetBeforeGen);
        if (GUILayout.Button("Reset Terrain")) terrain.ResetTerrain();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        serializedObject.ApplyModifiedProperties();
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
