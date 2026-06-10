using UnityEngine;
using GameData;
using UnityEngine.UI;
[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/Item", order = 1)]
public class Item : ScriptableObject
{
    new public string name;
    public string description;
    public int cost;
    public GameObject icon;
    public bool isConsumible;

    public ItemEffects[] effect;
    public AbilityCondition[] condition;
    public float[] effectChance;
    public Status statusToChangeTo;
    public Stance stanceToChangeTo;
    public int healingAmount;
    public ExecutionTime[] executionTime;
    public Stats[] statToMod;
    public float[] statMod;
    public bool affectSelf;


    public bool HasEffect(ItemEffects eff)
    {
        for (int i = 0; i < effect.Length; i++)
        {
            if (effect[i] == eff) return true;
        }

        return false;
    }
}
