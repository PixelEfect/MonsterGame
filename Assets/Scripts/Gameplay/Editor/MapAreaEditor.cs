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

        int totalChanceInGrass = serializedObject.FindProperty("totalChance").intValue;
        int totalChanceInWater = serializedObject.FindProperty("totalChanceWater").intValue;

        var style = new GUIStyle();
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 18;


        if(totalChanceInGrass == 100)
        {
            style.normal.textColor = Color.white;
            GUILayout.Label($"Total Chance In Grass = {totalChanceInGrass}", style);
        }

        if (totalChanceInGrass != 100 && totalChanceInGrass != -1)
        {
            style.normal.textColor = Color.red;
            GUILayout.Label($"Total Chance In Grass = {totalChanceInGrass}", style);
            EditorGUILayout.HelpBox("The total chance is not 100", MessageType.Error);
        }
        if (totalChanceInWater == 100)
        {
            style.normal.textColor = Color.white;
            GUILayout.Label($"Total Chance in Water = {totalChanceInWater}", style);
        }

        if (totalChanceInWater != 100 && totalChanceInWater != -1)
        {
            style.normal.textColor = Color.red;
            GUILayout.Label($"Total Chance in Water = {totalChanceInWater}", style);
            EditorGUILayout.HelpBox("The total chance is not 100", MessageType.Error);
        }
    }
}
