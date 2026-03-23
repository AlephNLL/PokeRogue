using Cinemachine;
using GameData;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

[CustomEditor(typeof(Unit))]
public class CustomUnitEditor : Editor
{
    #region SerializedProperties
    new SerializedProperty name;
    SerializedProperty description;
    SerializedProperty currentStance;
    SerializedProperty level;
    SerializedProperty status;
    SerializedProperty stanceModifier;

    SerializedProperty strength;
    SerializedProperty constitution;
    SerializedProperty dexterity;
    SerializedProperty luck;

    SerializedProperty maxHp;
    SerializedProperty currentHp;
    SerializedProperty attack;
    SerializedProperty effectiveAttack;
    SerializedProperty defense;
    SerializedProperty speed;

    SerializedProperty knownAbilities;
    SerializedProperty abilityPool;

    SerializedProperty actionCamera;
    SerializedProperty selectionCamera;
    SerializedProperty battleMenu;
    SerializedProperty abilityMenu;
    SerializedProperty attackButton;
    SerializedProperty runButton;
    SerializedProperty abilityButtons;
    SerializedProperty healthBar;
    SerializedProperty nameText;
    SerializedProperty isPlayerControlled;
    SerializedProperty additionalTurn;

    bool unitDataGroup;
    bool statsGroup;
    bool abilitiesGroup;
    bool miscGroup;
    #endregion

    private void OnEnable()
    {
        name = serializedObject.FindProperty("name");
        description = serializedObject.FindProperty("description");
        currentStance = serializedObject.FindProperty("currentStance");
        level = serializedObject.FindProperty("level");
        status = serializedObject.FindProperty("status");
        stanceModifier = serializedObject.FindProperty("stanceModifier");

        strength = serializedObject.FindProperty("strength");
        constitution = serializedObject.FindProperty("constitution");
        dexterity = serializedObject.FindProperty("dexterity");
        luck = serializedObject.FindProperty("luck");

        maxHp = serializedObject.FindProperty("maxHp");
        currentHp = serializedObject.FindProperty("currentHp");
        attack = serializedObject.FindProperty("attack");
        effectiveAttack = serializedObject.FindProperty("effectiveAttack");
        defense = serializedObject.FindProperty("defense");
        speed = serializedObject.FindProperty("speed");

        knownAbilities = serializedObject.FindProperty("knownAbilities");
        abilityPool = serializedObject.FindProperty("abilityPool");

        actionCamera = serializedObject.FindProperty("actionCamera");
        selectionCamera = serializedObject.FindProperty("selectionCamera");
        battleMenu = serializedObject.FindProperty("battleMenu");
        abilityMenu = serializedObject.FindProperty("abilityMenu");
        attackButton = serializedObject.FindProperty("attackButton");
        runButton = serializedObject.FindProperty("runButton");
        abilityButtons = serializedObject.FindProperty("abilityButtons");
        healthBar = serializedObject.FindProperty("healthBar");
        nameText = serializedObject.FindProperty("nameText");
        isPlayerControlled = serializedObject.FindProperty("isPlayerControlled");
        additionalTurn = serializedObject.FindProperty("additionalTurn");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        unitDataGroup = EditorGUILayout.Foldout(unitDataGroup, "Properties");

        if (unitDataGroup)
        {
            EditorGUILayout.PropertyField(name);
            EditorGUILayout.PropertyField(description);  
            EditorGUILayout.PropertyField(level);
            EditorGUILayout.PropertyField(status);
            EditorGUILayout.PropertyField(currentStance);
        }

        statsGroup = EditorGUILayout.Foldout(statsGroup, "Stats");
        if (statsGroup)
        {
            EditorGUILayout.PropertyField(strength);
            EditorGUILayout.PropertyField(constitution);
            EditorGUILayout.PropertyField(dexterity);
            EditorGUILayout.PropertyField(luck);

            EditorGUILayout.PropertyField(currentHp);
            EditorGUILayout.PropertyField(attack);
            EditorGUILayout.PropertyField(defense);
            EditorGUILayout.PropertyField(speed);
        }

        abilitiesGroup = EditorGUILayout.Foldout(abilitiesGroup, "Abilities");
        if (abilitiesGroup)
        {
            EditorGUILayout.PropertyField(knownAbilities);
            EditorGUILayout.PropertyField(abilityPool);
        }

        miscGroup = EditorGUILayout.Foldout(miscGroup, "Misc");
        if (miscGroup)
        {
            EditorGUILayout.PropertyField(actionCamera);
            EditorGUILayout.PropertyField(selectionCamera);
            EditorGUILayout.PropertyField(battleMenu);
            EditorGUILayout.PropertyField(abilityMenu);
            EditorGUILayout.PropertyField(attackButton);
            EditorGUILayout.PropertyField(runButton);
            EditorGUILayout.PropertyField(healthBar);
            EditorGUILayout.PropertyField(nameText);
            EditorGUILayout.PropertyField(abilityButtons);
            EditorGUILayout.PropertyField(isPlayerControlled);
            EditorGUILayout.PropertyField(additionalTurn);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
