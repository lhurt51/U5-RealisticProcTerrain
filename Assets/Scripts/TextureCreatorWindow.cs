using UnityEditor;
using UnityEngine;
using System.IO;

public class TextureCreatorWindow : EditorWindow {

    string filename = "myProceduralTexture";
    float perlinXScale = 0.001f;
    float perlinYScale = 0.001f;
    int perlinOctaves = 5;
    float perlinPersistance = 5.0f;
    float perlinHeightScale = 0.9f;
    int perlinOffsetX = 0;
    int PerlinOffsetY = 0;
    float brightness = 0.5f;
    float contrast = 0.5f;
    bool alphaToggle = false;
    bool seamlessToggle = false;
    bool remapToggle = false;

    Texture2D pTexture;

    private void GenerateTexture()
    {
        int w = 513;
        int h = 513;
        float pValue;
        float minColor = 1;
        float maxColor = 0;
        Color pixColor = Color.white;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (seamlessToggle)
                {
                    float u = (float)x / (float)w;
                    float v = (float)y / (float)h;
                    float noise00 = Utils.fBM((x + perlinOffsetX) * perlinXScale, (y + PerlinOffsetY) * perlinYScale, perlinOctaves, perlinPersistance) * perlinHeightScale;
                    float noise01 = Utils.fBM((x + perlinOffsetX) * perlinXScale, (y + PerlinOffsetY + h) * perlinYScale, perlinOctaves, perlinPersistance) * perlinHeightScale;
                    float noise10 = Utils.fBM((x + perlinOffsetX + w) * perlinXScale, (y + PerlinOffsetY) * perlinYScale, perlinOctaves, perlinPersistance) * perlinHeightScale;
                    float noise11 = Utils.fBM((x + perlinOffsetX + w) * perlinXScale, (y + PerlinOffsetY + h) * perlinYScale, perlinOctaves, perlinPersistance) * perlinHeightScale;
                    float noiseTotal = u * v * noise00 + u * (1 - v) * noise01 + (1 - u) * v * noise10 + (1 - u) * (1 - v) * noise11;
                    float value = (int)(256 * noiseTotal) + 50;
                    float r = Mathf.Clamp((int)noise00, 0, 255);
                    float g = Mathf.Clamp(value, 0, 255);
                    float b = Mathf.Clamp(value + 50, 0, 255);
                    float a = Mathf.Clamp(value + 100, 0, 255);

                    pValue = (r + g + b) / (3.0f * 255.0f);
                }
                else pValue = Utils.fBM((x + perlinOffsetX) * perlinXScale, (y + PerlinOffsetY) * perlinYScale, perlinOctaves, perlinPersistance) * perlinHeightScale;

                float colValue = contrast * (pValue - 0.5f) + 0.5f * brightness;
                if (minColor > colValue) minColor = colValue;
                if (maxColor < colValue) maxColor = colValue;
                pixColor = new Color(colValue, colValue, colValue, alphaToggle ? colValue : 1.0f);
                pTexture.SetPixel(x, y, pixColor);
            }
        }
        if (remapToggle)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    pixColor = pTexture.GetPixel(x, y);
                    float colVal = pixColor.r;
                    colVal = Utils.Map(colVal, minColor, maxColor, 0.0f, 1.0f);
                    pixColor.r = colVal;
                    pixColor.g = colVal;
                    pixColor.b = colVal;
                    pTexture.SetPixel(x, y, pixColor);
                }
            }
        }
        pTexture.Apply(false, false);
    }

    private void Save()
    {
        byte[] bytes = pTexture.EncodeToPNG();

        System.IO.Directory.CreateDirectory(Application.dataPath + "/SavedTextures");
        File.WriteAllBytes(Application.dataPath + "/SavedTextures/" + filename + ".png", bytes);
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
        brightness = EditorGUILayout.Slider("Brightness", brightness, 0.0f, 2.0f);
        contrast = EditorGUILayout.Slider("Contrast", contrast, 0.0f, 2.0f);
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

        if (GUILayout.Button("Save", GUILayout.Width(wSize))) Save();

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

}
