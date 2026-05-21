using GameData;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public AIAction CalculateBestAction(Unit controlledUnit, Unit[] opponents, Unit[] AIAllies)
    {
        Abilities[] activeAbilities = GetActiveAbilities(controlledUnit);

        Debug.Log($"<color=cyan>[AI] ========= Turno de: {controlledUnit.name} =========</color>");

        if (activeAbilities.Length == 0)
        {
            Debug.LogWarning($"[AI] {controlledUnit.name} no tiene habilidades activas disponibles. Saltando turno.");
            return new AIAction { skip = true };
        }

        AIScore[] scores = new AIScore[activeAbilities.Length];

        for (int i = 0; i < activeAbilities.Length; i++)
        {
            scores[i] = CalculateAbilityScores(controlledUnit, activeAbilities[i], opponents, AIAllies);

            for (int j = 0; j < scores[i].scores.Length; j++)
            {
                Debug.Log($"[AI] Evaluación -> Habilidad: <b>{activeAbilities[i].name}</b> | Objetivo: {scores[i].targets[j].name} | Puntuación Inicial: {scores[i].scores[j]}");
            }
        }

        //List<Unit> allies = new List<Unit>(AIAllies);
        //var tiedDamageOptions = new List<(AIScore scoreMap, int abilityIdx, int targetIdx)>();
        //int maxDamageFound = 0;

        // Recorremos de forma emparejada cada habilidad y cada uno de sus objetivos calculando su daño real
        //for (int i = 0; i < scores.Length; i++)
        //{
        //    for (int j = 0; j < scores[i].scores.Length; j++)
        //    {
        //        if (allies.Contains(scores[i].targets[j])) continue;

        //        int currentDmg = CalculateAttackDamage(controlledUnit, scores[i].targets[j], activeAbilities[i]);

        //        if (activeAbilities[i].power <= 0 || currentDmg <= 0) continue;

        //        if (currentDmg > maxDamageFound)
        //        {
        //            maxDamageFound = currentDmg;
        //            tiedDamageOptions.Clear();
        //            tiedDamageOptions.Add((scores[i], i, j));
        //        }
        //        else if (currentDmg == maxDamageFound)
        //        {
        //            tiedDamageOptions.Add((scores[i], i, j));
        //        }
        //    }
        //}

        //if (tiedDamageOptions.Count > 0)
        //{
        //    int randomIndex = UnityEngine.Random.Range(0, tiedDamageOptions.Count);
        //    var chosenOption = tiedDamageOptions[randomIndex];

        //    Unit target = chosenOption.scoreMap.targets[chosenOption.targetIdx];

        //    if (tiedDamageOptions.Count > 1)
        //    {
        //        Debug.Log($"<color=yellow>[AI] ¡Empate de daño detectado! Había {tiedDamageOptions.Count} opciones válidas que hacían el daño máximo de ({maxDamageFound}). " +
        //                  $"Se eligió al azar la habilidad <b>{activeAbilities[chosenOption.abilityIdx].name}</b> contra el objetivo <b>{target.name}</b>.</color>");
        //    }

        //    chosenOption.scoreMap.scores[chosenOption.targetIdx] += 6;

        //    Debug.Log($"<color=orange>[AI] ¡Bonus de Daño! Se aplica un +6 a <b>{activeAbilities[chosenOption.abilityIdx].name}</b> contra {target.name}.</color>");
        //}

        List<Abilities> bestAbilities = new List<Abilities>();
        List<Unit> bestTargets = new List<Unit>();
        int highestScoreValue = int.MinValue;

        for (int i = 0; i < scores.Length; i++)
        {
            for (int j = 0; j < scores[i].scores.Length; j++)
            {
                if (scores[i].scores[j] > highestScoreValue)
                {
                    highestScoreValue = scores[i].scores[j];
                    bestAbilities.Clear();
                    bestAbilities.Add(activeAbilities[i]);
                    bestTargets.Clear();
                    bestTargets.Add(scores[i].targets[j]);
                }

                if (scores[i].scores[j] == highestScoreValue)
                {
                    bestAbilities.Add(activeAbilities[i]);
                    bestTargets.Add(scores[i].targets[j]);
                }
            }
        }

        if(highestScoreValue < 0) return new AIAction { skip = true };

        int randomIndex = UnityEngine.Random.Range(0, bestAbilities.Count);

        Abilities bestAbility = bestAbilities[randomIndex];
        Unit bestTarget = bestTargets[randomIndex];

        AIAction finalAction = new AIAction();

        if (bestAbility != null && bestTarget != null)
        {
            finalAction.chosenAbility = bestAbility;
            finalAction.chosenTarget = bestTarget;
            finalAction.skip = false;

            Debug.Log($"<color=green>[AI] DECISIÓN FINAL: {controlledUnit.name} usará <b>{bestAbility.name}</b> sobre <b>{bestTarget.name}</b> con una puntuación total de {highestScoreValue}.</color>");
        }
        else
        {
            finalAction.skip = true;
            Debug.LogWarning($"[AI] No se pudo determinar una acción válida para {controlledUnit.name}. Saltando turno.");
        }

        Debug.Log("<color=cyan>[AI] =============================================</color>");
        return finalAction;
    }

    AIScore CalculateAbilityScores(Unit controlledUnit, Abilities ability, Unit[] opponents, Unit[] AIAllies)
    {
        AIScore score;

        if (ability.target == AbilityTarget.ONEENEMY || ability.target == AbilityTarget.ALLENEMIES)
        {
            score = new AIScore(new int[opponents.Length], opponents);
        }
        else if (ability.target == AbilityTarget.ONEALLY || ability.target == AbilityTarget.ALLALLIES)
        {
            score = new AIScore(new int[AIAllies.Length], AIAllies);
        }
        else if (ability.target == AbilityTarget.SELF)
        {
            Unit[] self = new Unit[1];
            self[0] = controlledUnit;
            score = new AIScore(new int[1], self);
        }
        else
        {
            List<Unit> temp = new List<Unit>(opponents);
            temp.AddRange(AIAllies);
            score = new AIScore(new int[temp.Count], temp.ToArray());
        }

        for (int i = 0; i < score.targets.Length; i++)
        {
            //Habilidades ofensivas
            if (ability.power > 0 && !ability.HasEffect(AbilityEffect.HEALATTACK))
            {
                if (CalculateAttackDamage(controlledUnit, score.targets[i], ability) >= score.targets[i].currentHp)
                {
                    score.scores[i] += 12;
                }
                if (ability.HasEffect(AbilityEffect.RAISEDCRITCHANCE) || ability.HasEffect(AbilityEffect.FLINCH))
                {
                    score.scores[i] += 1;
                }

                if(score.highestDMG < CalculateAttackDamage(controlledUnit, score.targets[i], ability))
                {
                    score.highestDMG = CalculateAttackDamage(controlledUnit, score.targets[i], ability);
                    score.highestDMGTargetIndex = i;
                }
            }

            //Cambios de estadísitcas
            if (ability.HasEffect(AbilityEffect.STATMOD))
            {
                int netStatsChange = 0;

                for (int j = 0; j < ability.statMod.Length; j++)
                {
                    if (ability.statMod[j] > 1) netStatsChange += 1;
                    else netStatsChange -= 1;
                }

                // A enemigos (Debufos / Nerfs)
                if ((ability.target == AbilityTarget.ONEENEMY || ability.target == AbilityTarget.ALLENEMIES) && !ability.affectSelf)
                {
                    if (netStatsChange < 0 && ability.GetAbilityEffectChance(AbilityEffect.STATMOD) == 100)
                    {
                        score.scores[i] += RandomScore(6, 8);
                    }
                    else if (netStatsChange < 0)
                    {
                        score.scores[i] += 1;
                    }
                }

                // Allies or self (Buffs)
                if (ability.target == AbilityTarget.SELF || ability.target == AbilityTarget.ONEALLY || ability.target == AbilityTarget.ALLALLIES || ability.affectSelf)
                {
                    if (netStatsChange >= 0)
                    {
                        int scalePenalty = 0;

                        for (int j = 0; j < ability.statToMod.Length; j++)
                        {
                            Stats targetStat = ability.statToMod[j];

                            int currentStatVal = score.targets[i].GetSetStat(targetStat);
                            int baseStatVal = score.targets[i].GetRawStat(targetStat, score.targets[i].level);

                            if (baseStatVal > 0)
                            {
                                float currentMultiplier = (float)currentStatVal / baseStatVal;
                                scalePenalty += Mathf.RoundToInt((currentMultiplier - 1.0f) * 4f / ability.statToMod.Length);
                            }
                        }
                        Debug.Log($"Buff penalty: {scalePenalty}");
                        if (ability.GetAbilityEffectChance(AbilityEffect.STATMOD) == 100)
                        {
                            // Apply the penalty directly to the random buff score
                            score.scores[i] += RandomScore(6, 8) - scalePenalty;
                        }
                        else
                        {
                            score.scores[i] += 1 - (scalePenalty / 2);
                        }
                    }
                    else if (netStatsChange < 0)
                    {
                        score.scores[i] -= 3;
                    }
                }
            }

            //Cambios de estado
            if (ability.HasEffect(AbilityEffect.APPLYSTATUS))
            {
                if ((ability.target == AbilityTarget.ONEENEMY || ability.target == AbilityTarget.ALLENEMIES) && !ability.affectSelf)
                {
                    if (ability.GetAbilityEffectChance(AbilityEffect.APPLYSTATUS) == 100)
                    {
                        score.scores[i] += RandomScore(6, 8);
                    }
                    else
                    {
                        score.scores[i] += 1;
                    }
                }

                if (ability.target == AbilityTarget.SELF || ability.target == AbilityTarget.ONEALLY || ability.target == AbilityTarget.ALLALLIES || ability.affectSelf)
                {
                    if (ability.status == Status.BURNED && ((!ability.affectSelf && score.targets[i].HasPassive("Pyromaniac")) || (ability.affectSelf && controlledUnit.HasPassive("Pyromaniac"))))
                    {
                        score.scores[i] += RandomScore(6, 8);
                    }
                    else if ((ability.affectSelf && controlledUnit.status == Status.NONE) || ((!ability.affectSelf && score.targets[i].status == Status.NONE)))
                    {
                        score.scores[i] -= 3;
                    }     
                }
            }

            //Cambios de postura
            if (ability.HasEffect(AbilityEffect.STANCECHANGE))
            {
                if ((ability.target == AbilityTarget.ONEENEMY || ability.target == AbilityTarget.ALLENEMIES) && !ability.affectSelf)
                {
                    if (GetUnusableAbilities(score.targets[i], ability.stanceToChangeTo).Length > 0)
                    {
                        score.scores[i] += 7;
                    }
                }

                if (ability.target == AbilityTarget.SELF || ability.target == AbilityTarget.ONEALLY || ability.target == AbilityTarget.ALLALLIES || ability.affectSelf)
                {
                    Abilities bestAbility = ability.affectSelf ? GetHighestPowerAbility(controlledUnit) : GetHighestPowerAbility(score.targets[i]);
                    if (bestAbility)
                    {
                        if (bestAbility.mustUseStance && ability.stanceToChangeTo == bestAbility.stance)
                        {
                            score.scores[i] += 6;
                        }
                        if (!(ability.target == AbilityTarget.ONEENEMY || ability.target == AbilityTarget.ALLENEMIES) && GetUnusableAbilities(score.targets[i], ability.stanceToChangeTo).Length > 2)
                        {
                            score.scores[i] -= 10;
                        }
                    }
                }
            }

            //Curaciones
            if (ability.HasEffect(AbilityEffect.HEAL) || ability.HasEffect(AbilityEffect.HEALATTACK))
            {
                if(score.targets[i].currentHp <= score.targets[i].maxHp / 2f)
                {
                    score.scores[i] += RandomScore(6, 8);
                }
            }

            //Ataques con condicion de primer turno
            if (ability.HasCondition(AbilityCondition.ISFIRSTROUND))
            {
                if (TBBS.instance.round == 0)
                {
                    score.scores[i] += 9;
                }
                else if (ability.power == 0)
                {
                    score.scores[i] -= 10;
                }
            }

            if (ability.target == AbilityTarget.ALL && AIAllies.Length > 0 && !controlledUnit.HasPassive("Empath"))
            {
                score.scores[i] -= 10;
            }
            else if(ability.target == AbilityTarget.ALL)
            {
                score.scores[i] += 3;
            }

            if (ability.HasEffect(AbilityEffect.SETEVASIVE) && AIAllies.Length > 0 && controlledUnit.currentHp <= controlledUnit.maxHp/2f)
            {
                score.scores[i] += RandomScore(6, 8);
            }

            if (ability.HasEffect(AbilityEffect.PROVOKE) && AIAllies.Length > 1 && controlledUnit.currentHp >= 3f*controlledUnit.maxHp / 2)
            {
                score.scores[i] += RandomScore(6, 8);
            }

            if (ability.HasEffect(AbilityEffect.SETGUARDIAN))
            {
                if (AIAllies.Length > 0 && controlledUnit.currentHp <= controlledUnit.maxHp / 2f && score.targets[i].constitution >= 16)
                {
                    score.scores[i] += RandomScore(6, 8);
                }
                else if (AIAllies.Length == 0)
                {
                    score.scores[i] -= 10;
                }
            }
        }

        return score;
    }

    Abilities GetHighestPowerAbility(Unit mon)
    {
        Abilities bestAbility = null;
        int bestPower = 0;

        for (int i = 0; i < mon.knownAbilities.Length; i++)
        {
            if (mon.knownAbilities[i].power > bestPower)
            {
                bestAbility = mon.knownAbilities[i];
                bestPower = mon.knownAbilities[i].power;
            }
        }

        return bestAbility;
    }
    Abilities[] GetUnusableAbilities(Unit mon, Stance stance)
    {
        Abilities[] abilities = mon.GetStanceLockedAbilities().ToArray();
        List<Abilities> unusableAbilities = new List<Abilities>();

        for (int i = 0; i < abilities.Length; i++)
        {
            if (abilities[i].mustUseStance && abilities[i].stance != stance)
            {
                unusableAbilities.Add(abilities[i]);
            }
        }

        return unusableAbilities.ToArray();
    }

    Abilities[] GetActiveAbilities(Unit mon)
    {
        List<Abilities> actives = new List<Abilities>();

        for (int i = 0; i < mon.knownAbilities.Length; i++)
        {
            if (mon.knownAbilities[i].abilityType == AbilityType.ACTIVE)
            {
                actives.Add(mon.knownAbilities[i]);
            }
        }

        return actives.ToArray();
    }
    int CalculateAttackDamage(Unit attacker, Unit target, Abilities ability)
    {
        int power = ability.power;
        switch (ability.powerVariables)
        {
            case AbilityPowerVariables.REMAININGHP:
                power = (int)(power * 5 * (1 - (float)attacker.currentHp / attacker.maxHp));
                break;
            case AbilityPowerVariables.DUPEONALLYDOWNED:
                power = (int)(power * Mathf.Pow(2, PlayerData.teamData.Count - TBBS.instance.playerUnits.Count));
                break;
            default:
                power = ability.power;
                break;
        }
        int attackStat = attacker.GetStat(ability.statToCalcDmgWith);
        int defenseStat = target.GetStat(Stats.DEF);
        float stanceBonus = attacker.currentStance == ability.stance ? 1.5f : 1;
        float efficacy = GetAbilityEfficacy(ability.stance, target.currentStance);
        float roll = UnityEngine.Random.Range(.8f, 1f);
        float freezeMod = target.status == Status.FROZEN ? 1.5f : 1f;

        float baseDamage = ((2 * attacker.level + 2) * .1f * power * attackStat) / (5.0f * defenseStat);
        float totalBeforeModifiers = baseDamage + 2;
        float finalDamageFloat = totalBeforeModifiers * efficacy * stanceBonus * roll * freezeMod;
        int damage = Mathf.FloorToInt(finalDamageFloat * target.recivedDamageMultiplier);

        if (damage <= 0) damage = 1;

        return damage;
    }

    float GetAbilityEfficacy(Stance abilityStance, Stance defenderStance)
    {
        switch (abilityStance)
        {
            case Stance.AGRESSIVE:
                if (defenderStance == Stance.AGILE) return 1.5f;
                else return 1;
            case Stance.DEFENSIVE:
                if (defenderStance == Stance.AGRESSIVE) return 1.5f;
                else return 1;
            case Stance.AGILE:
                if (defenderStance == Stance.DEFENSIVE) return 1.5f;
                else return 1;
            case Stance.CAUTIOUS:
                return 1;
            case Stance.TRICKY:
                return 1;
            default:
                return 1;
        }
    }

    int RandomScore(int value1, int value2)
    {
        if (Random.Range(0, 2) == 0) return value1;
        else return value2;
    }
}

public class AIAction
{
    public Abilities chosenAbility;
    public Unit chosenTarget;
    public bool skip;
}

public class AIScore
{
    public int[] scores;
    public Unit[] targets;
    public int highestDMG;
    public int highestDMGTargetIndex;

    public AIScore(int[] ints, Unit[] aIAllies)
    {
        this.scores = ints;
        this.targets = aIAllies;
    }
}
