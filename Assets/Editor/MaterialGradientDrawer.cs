using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MaterialGradient))]
public class MaterialGradientDrawer : PropertyDrawer {

    // static MapPreview mapPrev = null;

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        Event guiEvent = Event.current;
        MaterialGradient grad = (MaterialGradient)fieldInfo.GetValue(prop.serializedObject.targetObject);
        float labelWidth = GUI.skin.label.CalcSize(label).x + 5;
        Rect textRect = new Rect(pos.x + labelWidth, pos.y, pos.width - labelWidth, pos.height);

        // if (!mapPrev) mapPrev = GameObject.Find("MapPreview").GetComponent<MapPreview>();

        if (guiEvent.type == EventType.Repaint)
        {
            GUIStyle gradStyle = new GUIStyle();

            GUI.Label(pos, label);
            gradStyle.normal.background = grad.GetTexture((int)pos.width);
            GUI.Label(textRect, GUIContent.none, gradStyle);

            // if (mapPrev && mapPrev.autoUpdate) mapPrev.DrawMapInEditorGrad();
        }
        else if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && textRect.Contains(guiEvent.mousePosition))
        {
            // Open the window when clicked on
        }
    }

}
