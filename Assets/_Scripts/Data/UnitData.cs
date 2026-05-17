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
}
