using UnityEngine;
using GameData;
[CreateAssetMenu(fileName = "AbilityData", menuName = "ScriptableObjects/Ability", order = 1)]
public class Abilities : ScriptableObject
{
    [Header("Data")]
    new public string name;
    public string description;
    public int power;
    public int accuracy;
    public AbilityType abilityType;
    public AbilityTarget target;
    public Stance stance;
    public bool mustUseStance;

    [Header("PassiveData")]
    public PassiveEffects passiveEffects;
    public PassiveExecutionTime passiveExecutionTime;
    public AbilityTarget passiveTargets;

    [Header("Ability Effect")]
    public AbilityEffect effect1;
    public float effect1Chance;
    public AbilityEffect effect2;
    public float effect2Chance;
    public bool affectSelf;
}
