using System.Collections.Generic;
using GameData;

[System.Serializable]
public class UnitSaveData
{
    public int id;
    public string name;
    public string prefabId;
    public int level;
    public int currentExp;
    public string heldItemId;
    public int currentHp;
    public List<string> knownAbilityIds;
    public bool isVeteran;
    public Status status;
}
