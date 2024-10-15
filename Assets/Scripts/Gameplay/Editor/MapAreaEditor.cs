using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapArea))]

public class MapAreaEditor : Editor
{
    public override void OnInspectorGUI()
    {   
        //pokazuje domysle ui
        base.OnInspectorGUI();

        int totalChance = serializedObject.FindProperty("totalChance").intValue;

        var style = new GUIStyle();
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 18;


        if(totalChance == 100)
        {
            style.normal.textColor = Color.white;
            GUILayout.Label($"Total Chance = {totalChance}", style);
        }

        if (totalChance != 100)
        {
            style.normal.textColor = Color.red;
            GUILayout.Label($"Total Chance = {totalChance}", style);
            EditorGUILayout.HelpBox("The total chance is not 100", MessageType.Error);
        }
    }
}
