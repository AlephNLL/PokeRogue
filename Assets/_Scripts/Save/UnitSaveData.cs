using System.Collections.Generic;
using GameData;
using UnityEngine;

[System.Serializable]
public class UnitSaveData
{
    public int id;
    public string name;
    public GameObject prefab;
    public int level;
    public int currentExp;
    public Item heldItem;
    public int currentHp;
    public List<Abilities> knownAbilities;
    public bool isVeteran;
    public Status status;
}
