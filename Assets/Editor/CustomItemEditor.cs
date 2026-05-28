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
    SerializedProperty cost;
    SerializedProperty icon;
    SerializedProperty isConsumible;

    SerializedProperty effect;
    SerializedProperty condition;
    SerializedProperty effectChance;
    SerializedProperty statusToChangeTo;
    SerializedProperty stanceToChangeTo;
    SerializedProperty healingAmount;
    SerializedProperty statToMod;
    SerializedProperty statMod;
    SerializedProperty executionTime;
    SerializedProperty affectSelf;

    bool DataGroup;
    bool effectGroup;
    #endregion

    private void OnEnable()
    {
        name = serializedObject.FindProperty("name");
        description = serializedObject.FindProperty("description");
        cost = serializedObject.FindProperty("cost");
        icon = serializedObject.FindProperty("icon");
        isConsumible = serializedObject.FindProperty("isConsumible");

        effect = serializedObject.FindProperty("effect");
        condition = serializedObject.FindProperty("condition");
        effectChance = serializedObject.FindProperty("effectChance");
        statusToChangeTo = serializedObject.FindProperty("statusToChangeTo");
        stanceToChangeTo = serializedObject.FindProperty("stanceToChangeTo");
        healingAmount = serializedObject.FindProperty("healingAmount");
        statToMod = serializedObject.FindProperty("statToMod");
        statMod = serializedObject.FindProperty("statMod");
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
            EditorGUILayout.PropertyField(cost);
            EditorGUILayout.PropertyField(icon);
            EditorGUILayout.PropertyField(isConsumible);
        }

        effectGroup = EditorGUILayout.Foldout(effectGroup, "Item Effects");

        if (effectGroup)
        {
            EditorGUILayout.PropertyField(effect);
            if (!item.isConsumible) EditorGUILayout.PropertyField(condition);
            if (!item.isConsumible) EditorGUILayout.PropertyField(effectChance);
            for (int i = 0; i < item.effect.Length; i++)
            {
                if (item.effect[i] == ItemEffects.APPLYSTATUS) EditorGUILayout.PropertyField(statusToChangeTo);
                if (item.effect[i] == ItemEffects.CHANGESTANCEIFMOVESTANCE) EditorGUILayout.PropertyField(stanceToChangeTo);
                if (item.effect[i] == ItemEffects.HEAL) EditorGUILayout.PropertyField(healingAmount);
                if (item.effect[i] == ItemEffects.STATMOD || item.effect[i] == ItemEffects.INCREASESTAT) EditorGUILayout.PropertyField(statToMod);
                if (item.effect[i] == ItemEffects.STATMOD || item.effect[i] == ItemEffects.INCREASESTAT) EditorGUILayout.PropertyField(statMod);
            }
            
            if (!item.isConsumible) EditorGUILayout.PropertyField(executionTime);
            EditorGUILayout.PropertyField(affectSelf);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
