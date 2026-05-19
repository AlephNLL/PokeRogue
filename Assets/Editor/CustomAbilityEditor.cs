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
    SerializedProperty sfx;
    SerializedProperty power;
    SerializedProperty accuracy;
    SerializedProperty abilityType;
    SerializedProperty abilityTarget;
    SerializedProperty stance;
    SerializedProperty statToCalcDMGWith;
    SerializedProperty mustUseStance;
    SerializedProperty multiHit;
    SerializedProperty multiHitRange;
    SerializedProperty hits;
    SerializedProperty hitRange;
    SerializedProperty endOnMiss;
    SerializedProperty passiveEffects;
    SerializedProperty passiveExecutionTime;
    SerializedProperty passiveEffectChance;
    SerializedProperty status;

    SerializedProperty effect1;
    SerializedProperty condition1;
    SerializedProperty effect1Chance;
    SerializedProperty effect2;
    SerializedProperty condition2;
    SerializedProperty effect2Chance;
    SerializedProperty stanceToChangeTo;
    SerializedProperty conditionStance;
    SerializedProperty healingPercent;
    SerializedProperty statToMod;
    SerializedProperty statMod;
    SerializedProperty powerVariables;
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
        sfx = serializedObject.FindProperty("sfx");
        power = serializedObject.FindProperty("power");
        accuracy = serializedObject.FindProperty("accuracy");
        abilityType = serializedObject.FindProperty("abilityType");
        abilityTarget = serializedObject.FindProperty("target");
        stance = serializedObject.FindProperty("stance");
        statToCalcDMGWith = serializedObject.FindProperty("statToCalcDmgWith");
        mustUseStance = serializedObject.FindProperty("mustUseStance");
        multiHit = serializedObject.FindProperty("multiHit");
        multiHitRange = serializedObject.FindProperty("multiHitRange");
        hits = serializedObject.FindProperty("hits");
        hitRange = serializedObject.FindProperty("hitRange");
        endOnMiss = serializedObject.FindProperty("endOnMiss");

        passiveEffects = serializedObject.FindProperty("passiveEffects");
        passiveExecutionTime = serializedObject.FindProperty("passiveExecutionTime");
        passiveEffectChance = serializedObject.FindProperty("passiveEffectChance");
        status = serializedObject.FindProperty("status");

        effect1 = serializedObject.FindProperty("effect1");
        condition1 = serializedObject.FindProperty("condition1");
        effect1Chance = serializedObject.FindProperty("effect1Chance");
        effect2 = serializedObject.FindProperty("effect2");
        condition2 = serializedObject.FindProperty("condition2");
        effect2Chance = serializedObject.FindProperty("effect2Chance");
        stanceToChangeTo = serializedObject.FindProperty("stanceToChangeTo");
        conditionStance = serializedObject.FindProperty("stanceCondition");
        healingPercent = serializedObject.FindProperty("healPercent");
        statMod = serializedObject.FindProperty("statMod");
        statToMod = serializedObject.FindProperty("statToMod");
        powerVariables = serializedObject.FindProperty("powerVariables");
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
            EditorGUILayout.PropertyField(sfx);
            EditorGUILayout.PropertyField(power);
            EditorGUILayout.PropertyField(accuracy);
            EditorGUILayout.PropertyField(abilityType);
            EditorGUILayout.PropertyField(abilityTarget);
            EditorGUILayout.PropertyField(stance);
            if (ability.power > 0) EditorGUILayout.PropertyField(statToCalcDMGWith);
            EditorGUILayout.PropertyField(mustUseStance);
            EditorGUILayout.PropertyField(multiHit);
            EditorGUILayout.PropertyField(multiHitRange);
            if (ability.multiHit) EditorGUILayout.PropertyField(hits);
            if (ability.multiHitRange) EditorGUILayout.PropertyField(hitRange);
            if (ability.multiHitRange || ability.multiHit) EditorGUILayout.PropertyField(endOnMiss);
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
                if (ability.effect1 != AbilityEffect.NONE) EditorGUILayout.PropertyField(condition1);
                if (ability.effect1 != AbilityEffect.NONE) EditorGUILayout.PropertyField(effect1Chance);   
                EditorGUILayout.PropertyField(effect2);
                if (ability.effect2 != AbilityEffect.NONE) EditorGUILayout.PropertyField(condition2);
                if (ability.effect2 != AbilityEffect.NONE) EditorGUILayout.PropertyField(effect2Chance);
                if (ability.effect1 == AbilityEffect.APPLYSTATUS || ability.effect2 == AbilityEffect.APPLYSTATUS) EditorGUILayout.PropertyField(status);
                if (ability.effect1 == AbilityEffect.STANCECHANGE || ability.effect2 == AbilityEffect.STANCECHANGE) EditorGUILayout.PropertyField(stanceToChangeTo);
                if(ability.effect1 == AbilityEffect.STANCECHANGE && ability.condition1 == AbilityCondition.HASSTANCE ||
                    ability.effect2 == AbilityEffect.STANCECHANGE && ability.condition2 == AbilityCondition.HASSTANCE) EditorGUILayout.PropertyField(conditionStance);
                if (ability.effect1 == AbilityEffect.HEAL || ability.effect2 == AbilityEffect.HEAL) EditorGUILayout.PropertyField(healingPercent);
                if (ability.effect1 == AbilityEffect.STATMOD || ability.effect2 == AbilityEffect.STATMOD || ability.effect1 == AbilityEffect.SWAPSTATS || ability.effect2 == AbilityEffect.SWAPSTATS) EditorGUILayout.PropertyField(statToMod);
                if (ability.effect1 == AbilityEffect.STATMOD || ability.effect2 == AbilityEffect.STATMOD) EditorGUILayout.PropertyField(statMod);
                if (ability.effect1 == AbilityEffect.VARIABLEPOWER || ability.effect2 == AbilityEffect.VARIABLEPOWER) EditorGUILayout.PropertyField(powerVariables);
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
