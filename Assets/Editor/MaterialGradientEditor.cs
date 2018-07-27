using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MaterialGradientEditor : EditorWindow {

    MaterialGradient gradient;

    // Constant parameters for sizes
    const int borderSize = 10;
    const float keyWidth = 10.0f;
    const float keyHeight = 20.0f;

    Rect gradPrevRect;
    Rect[] matRects;
    bool mouseIsOverKey;
    bool shouldRepaint;
    int selectedKeyIndex;

    public MaterialGradient Gradient
    {
        get { return gradient; }
        set { gradient = value; }
    }

    private int MatIndex
    {
        get { return Mathf.FloorToInt(selectedKeyIndex * 0.5f); }
    }

    private void OnEnable()
    {
        titleContent.text = "Materials Gradient Editor";
        position.Set(position.x, position.y, 400, 250);
        minSize = new Vector2(200, 500);
        maxSize = new Vector2(1920, 500);
    }

    private void OnDisable()
    {
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    private void OnGUI()
    {
        Draw();
        HandleInput();

        if (shouldRepaint)
        {
            shouldRepaint = false;
            Repaint();
        }
    }

    // Need a way to update both the min and max height through keys
    private void Draw()
    {
        gradPrevRect = new Rect(borderSize, borderSize, position.width - borderSize * 2, 25);
        GUI.DrawTexture(gradPrevRect, gradient.GetTexture((int)gradPrevRect.width));
        matRects = new Rect[gradient.NumMats * 2];
        for (int i = 0; i < gradient.NumMats * 2; i++)
        {
            MaterialGradient.MaterialLevel mat = gradient.GetMatLevel(Mathf.FloorToInt(i * 0.5f));
            Rect matRect = new Rect(gradPrevRect.x + gradPrevRect.width * ((i % 2 == 0) ? mat.MinHeight : mat.MaxHeight) - keyWidth / 2.0f, gradPrevRect.yMax + borderSize, keyWidth, keyHeight);

            if (i == selectedKeyIndex)
                EditorGUI.DrawRect(new Rect(matRect.x - 2, matRect.y - 2, matRect.width + 4, matRect.height + 4), Color.black);
            EditorGUI.DrawRect(matRect, mat.Tint);
            matRects[i] = matRect;
        }

        Rect settingsRect = new Rect(borderSize, matRects[0].yMax + borderSize, position.width - borderSize * 2, position.height - borderSize);
        GUILayout.BeginArea(settingsRect);

        EditorGUI.BeginChangeCheck();

        Texture2D newText = (Texture2D)EditorGUILayout.ObjectField("Texture", gradient.GetMatLevel(MatIndex).Texture, typeof(Texture2D), false);
        Color newTint = EditorGUILayout.ColorField(gradient.GetMatLevel(MatIndex).Tint);
        float newTintStrength = EditorGUILayout.Slider("Tint Strength", gradient.GetMatLevel(MatIndex).TintStrength, 0.0f, 1.0f);
        float newMinSlope = EditorGUILayout.Slider("Min Slope", gradient.GetMatLevel(MatIndex).MinSlope, 0.0f, 90.0f);
        float newMaxSlope = EditorGUILayout.Slider("Max Slope", gradient.GetMatLevel(MatIndex).MaxSlope, 0.0f, 90.0f);
        float newBlendStrength = EditorGUILayout.Slider("Blend Strength", gradient.GetMatLevel(MatIndex).BlendStrength, 0.0f, 1.0f);
        Vector2 newTileOffset = EditorGUILayout.Vector2Field("Tex Offset", gradient.GetMatLevel(MatIndex).TileOffset);
        Vector2 newTileScale = EditorGUILayout.Vector2Field("Tex Scale", gradient.GetMatLevel(MatIndex).TileScale);
        Vector2 newSplatNoiseVScale = EditorGUILayout.Vector2Field("Blend Noise Vector Scale", gradient.GetMatLevel(MatIndex).SplatNoiseVScale);
        float newSplatNoiseScaler = EditorGUILayout.FloatField("Blend Noise Scaler", gradient.GetMatLevel(MatIndex).SplatNoiseScaler);

        if (EditorGUI.EndChangeCheck())
        {
            gradient.UpdateMatTexture(MatIndex, newText);
            gradient.UpdateMatTint(MatIndex, newTint);
            gradient.UpdateMatTintStrength(MatIndex, newTintStrength);
            gradient.UpdateMatMinSlope(MatIndex, newMinSlope);
            gradient.UpdateMatMaxSlope(MatIndex, newMaxSlope);
            gradient.UpdateMatBlendStrength(MatIndex, newBlendStrength);
            gradient.UpdateMatTileOffset(MatIndex, newTileOffset);
            gradient.UpdateMatTileScale(MatIndex, newTileScale);
            gradient.UpdateMatSplatNoiseVScale(MatIndex, newSplatNoiseVScale);
            gradient.UpdateMatSplatNoiseScaler(MatIndex, newSplatNoiseScaler);
        }
        gradient.bRandomizeTint = EditorGUILayout.Toggle("Randomize Tint", gradient.bRandomizeTint);

        GUILayout.EndArea();
    }

    private void HandleInput()
    {
        Event guiEvent = Event.current;

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
        {
            for (int i = 0; i < matRects.Length; i++)
            {
                if (matRects[i].Contains(guiEvent.mousePosition))
                {
                    selectedKeyIndex = i;
                    mouseIsOverKey = true;
                    shouldRepaint = true;
                    break;
                }
            }

            if (!mouseIsOverKey && matRects.Length < 14)
            {
                float keyTime = Mathf.InverseLerp(gradPrevRect.x, gradPrevRect.xMax, guiEvent.mousePosition.x);
                Color interpColor = gradient.Eval(keyTime);
                Color randColor = new Color(Random.value, Random.value, Random.value);

                selectedKeyIndex = gradient.AddMat((gradient.bRandomizeTint) ? randColor : interpColor, keyTime) * 2;
                mouseIsOverKey = true;
                shouldRepaint = true;
            }
        }
        else if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0) mouseIsOverKey = false;
        else if (mouseIsOverKey && guiEvent.type == EventType.MouseDrag && guiEvent.button == 0)
        {
            float keyTime = Mathf.InverseLerp(gradPrevRect.x, gradPrevRect.xMax, guiEvent.mousePosition.x);

            selectedKeyIndex = (selectedKeyIndex % 2 == 0) ? gradient.UpdateMatMinHeight(MatIndex, keyTime) * 2 : gradient.UpdateMatMaxHeight(MatIndex, keyTime) * 2 + 1;
            shouldRepaint = true;
        }
        else if (guiEvent.keyCode == KeyCode.Backspace && guiEvent.type == EventType.KeyDown)
        {
            if (selectedKeyIndex >= gradient.NumMats * 2) selectedKeyIndex -= 2;
            gradient.RemoveMat(MatIndex);
            if (selectedKeyIndex >= gradient.NumMats * 2) selectedKeyIndex -= 2;
            shouldRepaint = true;
        }
    }

}
