using UnityEngine;
using GameData;
using UnityEngine.UI;
[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/Item", order = 1)]
public class Item : ScriptableObject
{
    new public string name;
    public string description;
    public Image icon;
    public bool isConsumible;

    public ItemEffects effect;
    public float effectChance;
    public Status statusToChangeTo;
    public int healingAmount;
    public ExecutionTime executionTime;
    public bool affectSelf;
}
