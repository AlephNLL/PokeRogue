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
}
