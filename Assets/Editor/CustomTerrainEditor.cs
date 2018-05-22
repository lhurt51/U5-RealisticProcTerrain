using UnityEngine;
using UnityEditor;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor {

    // Properties ---------
    // The height range used for the rand gen
    SerializedProperty randomHeightRange;
    // The height map scale used for gen
    SerializedProperty heightMapScale;
    // The height map image used for gen
    SerializedProperty heightMapImage;

    // Fold outs ----------
    // Fold out for the random hieght generation properties
    bool showRandom = false;
    // Flod out for the image import for heights
    bool showLoadHeights = false;

    // To allow us to recompile in editor without playing
    void OnEnable()
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;

        showRandom = EditorGUILayout.Foldout(showRandom, "RandomGenProps");
        if (showRandom)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Terrain Height Randomly Between Two Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange);
            if (GUILayout.Button("Gen Random Heights")) terrain.RandomTerrain();
        }

        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights, "ImageGenProps");
        if (showLoadHeights)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Load Heights From Texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightMapImage);
            EditorGUILayout.PropertyField(heightMapScale);
            if (GUILayout.Button("Load Texture")) terrain.LoadTexture();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Reset Terrain")) terrain.ResetTerrain();

        serializedObject.ApplyModifiedProperties();
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
