using GameData;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public AIAction CalculateBestAction(Unit controlledUnit, Unit[] opponents, Unit[] AIAllies)
    {
        for (int i = 0; i < controlledUnit.knownAbilities.length; i++)
        {
            
        }
    }

    AIScore CalculateAbilityScores(Abilities ability, Unit[] opponents, Unit[] AIAllies)
    {
        AIScore score;

        if(ability.target == AbilityTarget.ONEENEMY || ability.target == AbilityTarget.ALLENEMIES)
        {
            score = new AIScore(new int[opponents.length], opponents);
        }
        else if(ability.target == AbilityTarget.ONEALLY || ability.target == AbilityTarget.ALLALLIES)
        {
            score = new AIScore(new int[AIAllies.length], AIAllies);
        }
        else
        {
            List<Unit> temp = opponents;
            temp.AddRange(AIAllies);
            score = new AIScore(new int[temp.Count], temp.ToArray());
        }
        if (ability.power > 0)
        {
            if(ability.HasEffect(AbilityEffect.RAISEDCRITCHANCE) || ability.HasEffect(AbilityEffect.FLINCH))
            {

            }
        }
    }
}

public class AIAction
{
    public Abilities chosenAbility;
    public Unit chosenTarget;
}

public class AIScore
{
    public int[] scores;
    public Unit[] targets;
}
