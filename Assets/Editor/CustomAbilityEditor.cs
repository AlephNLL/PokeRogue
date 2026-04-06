using Cinemachine;
using GameData;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

[CustomEditor(typeof(Abilities))]
public class CustomAbilityEditor : Editor
{
    #region SerializedProperties
    new SerializedProperty name;
    SerializedProperty description;
    SerializedProperty power;
    SerializedProperty accuracy;
    SerializedProperty abilityType;
    SerializedProperty abilityTarget;
    SerializedProperty stance;
    SerializedProperty mustUseStance;

    SerializedProperty passiveEffects;
    SerializedProperty passiveExecutionTime;
    SerializedProperty passiveEffectChance;
    SerializedProperty status;

    SerializedProperty effect1;
    SerializedProperty effect1Chance;
    SerializedProperty effect2;
    SerializedProperty effect2Chance;
    SerializedProperty stanceToChangeTo;
    SerializedProperty affectSelf;

    SerializedProperty vfxPrefab;
    SerializedProperty spawnVfxOnSelf;

    bool DataGroup;
    bool statsGroup;
    bool abilitiesGroup;
    bool miscGroup;
    #endregion

    private void OnEnable()
    {
        name = serializedObject.FindProperty("name");
        description = serializedObject.FindProperty("description");
        power = serializedObject.FindProperty("power");
        accuracy = serializedObject.FindProperty("accuracy");
        abilityType = serializedObject.FindProperty("abilityType");
        abilityTarget = serializedObject.FindProperty("target");
        stance = serializedObject.FindProperty("stance");
        mustUseStance = serializedObject.FindProperty("mustUseStance");

        passiveEffects = serializedObject.FindProperty("passiveEffects");
        passiveExecutionTime = serializedObject.FindProperty("passiveExecutionTime");
        passiveEffectChance = serializedObject.FindProperty("passiveEffectChance");
        status = serializedObject.FindProperty("status");

        effect1 = serializedObject.FindProperty("effect1");
        effect1Chance = serializedObject.FindProperty("effect1Chance");
        effect2 = serializedObject.FindProperty("effect2");
        effect2Chance = serializedObject.FindProperty("effect2Chance");
        stanceToChangeTo = serializedObject.FindProperty("stanceToChangeTo");
        affectSelf = serializedObject.FindProperty("affectSelf");

        vfxPrefab = serializedObject.FindProperty("vfxPrefab");
        spawnVfxOnSelf = serializedObject.FindProperty("spawnVfxOnSelf");
    }

    public override void OnInspectorGUI()
    {
        Abilities ability = (Abilities)target;

        serializedObject.Update();

        DataGroup = EditorGUILayout.Foldout(DataGroup, "Data");

        if (DataGroup)
        {
            EditorGUILayout.PropertyField(name);
            EditorGUILayout.PropertyField(description);  
            EditorGUILayout.PropertyField(power);
            EditorGUILayout.PropertyField(accuracy);
            EditorGUILayout.PropertyField(abilityType);
            EditorGUILayout.PropertyField(abilityTarget);
            EditorGUILayout.PropertyField(stance);
            EditorGUILayout.PropertyField(mustUseStance);
        }

        if (ability.abilityType == AbilityType.PASSIVE)
        {
            statsGroup = EditorGUILayout.Foldout(statsGroup, "Passive Effects");
            if (statsGroup)
            {
                EditorGUILayout.PropertyField(passiveEffects);
                if(ability.passiveEffects == PassiveEffects.APPLYSTATUS || ability.passiveEffects == PassiveEffects.UPATKONSTATUS) EditorGUILayout.PropertyField(status);
                EditorGUILayout.PropertyField(passiveExecutionTime);
                EditorGUILayout.PropertyField(passiveEffectChance);
            }
        }
        else
        {
            abilitiesGroup = EditorGUILayout.Foldout(abilitiesGroup, "Active Effects");
            if (abilitiesGroup)
            {
                EditorGUILayout.PropertyField(effect1);
                if (ability.effect1 != AbilityEffect.NONE) EditorGUILayout.PropertyField(effect1Chance);   
                EditorGUILayout.PropertyField(effect2);
                if (ability.effect2 != AbilityEffect.NONE) EditorGUILayout.PropertyField(effect2Chance);
                if (ability.effect1 == AbilityEffect.APPLYSTATUS || ability.effect2 == AbilityEffect.APPLYSTATUS) EditorGUILayout.PropertyField(status);
                if (ability.effect1 == AbilityEffect.STANCECHANGE || ability.effect2 == AbilityEffect.STANCECHANGE) EditorGUILayout.PropertyField(stanceToChangeTo);
                EditorGUILayout.PropertyField(affectSelf);
            }
        }  

        miscGroup = EditorGUILayout.Foldout(miscGroup, "VFX");
        if (miscGroup)
        {
            EditorGUILayout.PropertyField(vfxPrefab);
            EditorGUILayout.PropertyField(spawnVfxOnSelf);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
