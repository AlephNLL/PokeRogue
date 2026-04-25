using Cinemachine;
using GameData;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

[CustomEditor(typeof(Item))]
public class CustomItemEditor : Editor
{
    #region SerializedProperties
    new SerializedProperty name;
    SerializedProperty description;
    SerializedProperty icon;
    SerializedProperty isConsumible;

    SerializedProperty effect;
    SerializedProperty effectChance;
    SerializedProperty statusToChangeTo;
    SerializedProperty executionTime;
    SerializedProperty affectSelf;

    bool DataGroup;
    bool effectGroup;
    #endregion

    private void OnEnable()
    {
        name = serializedObject.FindProperty("name");
        description = serializedObject.FindProperty("description");
        icon = serializedObject.FindProperty("icon");
        isConsumible = serializedObject.FindProperty("isConsumible");

        effect = serializedObject.FindProperty("effect");
        effectChance = serializedObject.FindProperty("effectChance");
        statusToChangeTo = serializedObject.FindProperty("statusToChangeTo");
        executionTime = serializedObject.FindProperty("executionTime");
        affectSelf = serializedObject.FindProperty("affectSelf");
    }

    public override void OnInspectorGUI()
    {
        Item item = (Item)target;

        serializedObject.Update();

        DataGroup = EditorGUILayout.Foldout(DataGroup, "Data");

        if (DataGroup)
        {
            EditorGUILayout.PropertyField(name);
            EditorGUILayout.PropertyField(description);
            EditorGUILayout.PropertyField(icon);
            EditorGUILayout.PropertyField(isConsumible);
        }

        effectGroup = EditorGUILayout.Foldout(effectGroup, "Item Effects");

        if (effectGroup)
        {
            EditorGUILayout.PropertyField(effect);
            if (!item.isConsumible) EditorGUILayout.PropertyField(effectChance);
            if (item.effect == ItemEffects.APPLYSTATUS) EditorGUILayout.PropertyField(statusToChangeTo);
            if (!item.isConsumible) EditorGUILayout.PropertyField(executionTime);
            EditorGUILayout.PropertyField(affectSelf);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
