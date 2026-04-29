using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Scriptable Objects/UnitData")]
public class UnitData : ScriptableObject
{
    public int id;
    new public string name;
    public GameObject prefab;
    public int level;
    public Item heldItem;
    public int currentHp;
    public Abilities[] knownAbilities;
    public bool isVeteran;
}
