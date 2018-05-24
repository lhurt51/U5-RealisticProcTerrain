using UnityEditor;
using UnityEngine;
using System.IO;

public class TextureCreatorWindow : EditorWindow {

    string filename = "myProceduralTexture";
    float perlinXScale = 0.01f;
    float perlinYScale = 0.01f;
    int perlinOctaves = 3;
    float perlinPersistance = 8.0f;
    float perlinHeightScale = 0.09f;
    int perlinOffsetX = 0;
    int PerlinOffsetY = 0;
    bool alphaToggle = false;
    bool seamlessToggle = false;
    bool remapToggle = false;

    Texture2D pTexture;

    private void GenerateTexture()
    {
        int w = 513;
        int h = 513;
        float pValue;
        Color pixColor = Color.white;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                pValue = Utils.fBM((x + perlinOffsetX) * perlinXScale, (y + PerlinOffsetY) * perlinYScale, perlinOctaves, perlinPersistance) * perlinHeightScale;
                float colValue = pValue;
                pixColor = new Color(colValue, colValue, colValue, alphaToggle ? colValue : 1.0f);
                pTexture.SetPixel(x, y, pixColor);
            }
        }
        pTexture.Apply(false, false);
    }

	[MenuItem("Window/TextureCreatorWindow")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TextureCreatorWindow));
    }

    private void OnEnable()
    {
        pTexture = new Texture2D(513, 513, TextureFormat.ARGB32, false);
    }

    private void OnGUI()
    {
        int wSize = (int)(EditorGUIUtility.currentViewWidth - 100);

        GUILayout.Label("Settings", EditorStyles.boldLabel);
        filename = EditorGUILayout.TextField("Texture Name", filename);

        perlinXScale = EditorGUILayout.Slider("X Scale", perlinXScale, 0.0f, 0.1f);
        perlinYScale = EditorGUILayout.Slider("Y Scale", perlinYScale, 0.0f, 0.1f);
        perlinOctaves = EditorGUILayout.IntSlider("Octaves", perlinOctaves, 1, 10);
        perlinPersistance = EditorGUILayout.Slider("Persistance", perlinPersistance, 1.0f, 10.0f);
        perlinHeightScale = EditorGUILayout.Slider("Height Scale", perlinHeightScale, 0.0f, 1.0f);
        perlinOffsetX = EditorGUILayout.IntSlider("Offset X", perlinOffsetX, 0, 10000);
        PerlinOffsetY = EditorGUILayout.IntSlider("Offset Y", PerlinOffsetY, 0, 10000);
        alphaToggle = EditorGUILayout.Toggle("Alpha?", alphaToggle);
        remapToggle = EditorGUILayout.Toggle("Remap?", remapToggle);
        seamlessToggle = EditorGUILayout.Toggle("Seamless", seamlessToggle);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Generate", GUILayout.Width(wSize))) GenerateTexture();

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUILayout.Label(pTexture, GUILayout.Width(wSize), GUILayout.Height(wSize));

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Save", GUILayout.Width(wSize))) { }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

}
