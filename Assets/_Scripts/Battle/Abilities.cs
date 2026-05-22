using UnityEngine;
using GameData;
[CreateAssetMenu(fileName = "AbilityData", menuName = "Scriptable Objects/Ability", order = 1)]
public class Abilities : ScriptableObject
{
    new public string name;
    public string description;
    public SoundData sfx;
    public int power;
    public int accuracy;
    public AbilityType abilityType;
    public AbilityTarget target;
    public Stance stance;
    public Stats statToCalcDmgWith = Stats.ATK;
    public bool mustUseStance;
    public bool multiHit;
    public bool multiHitRange;
    public int hits;
    public int[] hitRange;
    public bool endOnMiss;

    public PassiveEffects passiveEffects;
    public ExecutionTime passiveExecutionTime;
    public int passiveEffectChance;
    public Status status;

    public AbilityEffect effect1;
    public AbilityCondition condition1;
    public float effect1Chance;
    public AbilityEffect effect2;
    public AbilityCondition condition2;
    public float effect2Chance;
    public bool affectSelf;
    public Stance stanceToChangeTo;
    public Stance stanceCondition;
    public float healPercentage;
    public Stats[] statToMod;
    public float[] statMod;
    public AbilityPowerVariables powerVariables;

    public GameObject vfxPrefab;
    public bool spawnVfxOnSelf;

    public bool HasEffect(AbilityEffect effect)
    {
        return effect1 == effect || effect2 == effect;
    }

    public bool HasCondition(AbilityCondition condition)
    {
        return condition1 == condition || condition2 == condition;
    }
    public float GetAbilityEffectChance(AbilityEffect effect)
    {
        if (effect1 == effect)
        {
            return effect1Chance;
        }
        else if (effect2 == effect)
        {
            return effect2Chance;
        }
        else
        {
            return 0;
        }
    }
}
