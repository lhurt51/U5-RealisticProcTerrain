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

    private void OnEnable()
    {
        titleContent.text = "Materials Gradient Editor";
        position.Set(position.x, position.y, 400, 250);
        minSize = new Vector2(200, 250);
        maxSize = new Vector2(1920, 250);
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

    private void Draw()
    {

    }

    private void HandleInput()
    {

    }

}
