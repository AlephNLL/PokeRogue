using GameData;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Scriptable Objects/UnitData")]
public class UnitData : ScriptableObject
{
    public int id;
    new public string name;
    public GameObject prefab;
    public int level;
    public int currentExp;
    public Item heldItem;
    public int currentHp;
    public Status status;
    public List<Abilities> knownAbilities;
    public bool isVeteran;

    public bool HasAbility(string name)
    {
        for (int i = 0; i < knownAbilities.Count; i++)
        {
            if (knownAbilities[i].name == name) return true;
        }
        return false;
    }
    public UnitSaveData LoadData()
    {
        UnitSaveData data = new UnitSaveData();
        data.id = id;
        data.name = name;
        data.prefab = prefab;
        data.level = level;
        data.currentExp = currentExp;
        data.heldItem = heldItem;
        data.currentHp = currentHp;
        data.knownAbilities = knownAbilities;
        data.isVeteran = isVeteran;
        return data;
    }

    public UnitData SaveData(UnitSaveData data)
    {
        id = data.id;
        name = data.name;
        prefab = data.prefab;
        level = data.level;
        currentExp = data.currentExp;
        heldItem = data.heldItem;
        currentHp = data.currentHp;
        knownAbilities = data.knownAbilities;
        isVeteran = data.isVeteran;
        return this;
    }
}
