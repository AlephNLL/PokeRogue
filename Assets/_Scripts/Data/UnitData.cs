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

    public void HoldItem(Item item)
    {
        if (heldItem == null)
        {
            heldItem = item;
            PlayerData.Instance.p_items.Remove(item);
        }
        else
        {
            if (item != null) { PlayerData.Instance.p_items.Remove(item); }
            PlayerData.Instance.p_items.Add(heldItem);
            heldItem = item;
        }
    }

    public void ConsumeItem(Item item, int monsIndex)
    {
        PlayerData.Instance.p_items.Remove(item);

        switch (item.effect[0])
        {
            case ItemEffects.CURESTATUS:
                status = Status.NONE;
                FresnelApplier.clearFresnel(MapView.instance.team[monsIndex]);
                break;
            case ItemEffects.HEAL:
                currentHp += item.healingAmount;
                break;
            case ItemEffects.LEVELUP:
                level += 1;
                MoveLearner.instance.LearnMove(this, level);
                break;
            default:
                break;

        }
    }

    public UnitSaveData LoadData()
    {
        UnitSaveData data = new UnitSaveData();
        data.id = id;
        data.name = name;
        data.prefabId = SaveReferenceDatabase.GetId(prefab);
        data.level = level;
        data.currentExp = currentExp;
        data.heldItemId = SaveReferenceDatabase.GetId(heldItem);
        data.currentHp = currentHp;
        data.knownAbilityIds = new List<string>();
        if (knownAbilities != null)
        {
            foreach (Abilities ability in knownAbilities)
            {
                data.knownAbilityIds.Add(SaveReferenceDatabase.GetId(ability));
            }
        }
        data.isVeteran = isVeteran;
        data.status = status;
        return data;
    }

    public UnitData SaveData(UnitSaveData data)
    {
        id = data.id;
        name = data.name;
        prefab = SaveReferenceDatabase.Instance != null ? SaveReferenceDatabase.Instance.GetUnitPrefab(data.prefabId) : null;
        level = data.level;
        currentExp = data.currentExp;
        heldItem = SaveReferenceDatabase.Instance != null ? SaveReferenceDatabase.Instance.GetItem(data.heldItemId) : null;
        currentHp = data.currentHp;
        knownAbilities = new List<Abilities>();
        if (data.knownAbilityIds != null && SaveReferenceDatabase.Instance != null)
        {
            foreach (string abilityId in data.knownAbilityIds)
            {
                Abilities ability = SaveReferenceDatabase.Instance.GetAbility(abilityId);
                if (ability != null) knownAbilities.Add(ability);
            }
        }
        isVeteran = data.isVeteran;
        status = data.status;
        return this;
    }
}
