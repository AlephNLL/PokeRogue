using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator), true)]
public class MapGeneratorEditor : Editor
{
    MapGenerator generator;

    private void Awake()
    {
        generator = (MapGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate Level"))
        {
            generator.GenerateMap();
        }
    }
}
