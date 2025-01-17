using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Heaton.WorldGen;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = (MapGenerator)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate Map"))
        {
            mapGen.GenerateMap();
        }
    }
}
