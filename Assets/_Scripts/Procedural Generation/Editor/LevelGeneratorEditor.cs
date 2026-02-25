using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AbstractLevelGenerator), true)]
public class LevelGeneratorEditor : Editor
{
    AbstractLevelGenerator generator;

    private void Awake()
    {
        generator = (AbstractLevelGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Generate Level"))
        {
            generator.GenerateLevel();
        }
    }
}
