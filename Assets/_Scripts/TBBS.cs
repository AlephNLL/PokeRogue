using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using System;
using GameData;
using UnityEngine.SceneManagement;


//Turn Based Battle System
public enum BattleState { START, PLAYERTURN, ENEMYTURN, LOSS, WIN }
public class TBBS : MonoBehaviour
{
    public static TBBS instance;
    public GameObject[] playerPrefabs;
    public GameObject[] enemyPrefabs;

    public Transform playerSide;
    public Transform enemySide;

    public List<Unit> playerUnits;
    public List<Unit> enemyUnits;

    private BattleState battleState;
    private int currentTurnIndex = 0;
    private int round = 0;

    public List<Unit> allUnits;

    private Coroutine currentTurnCoroutine;

    private void Awake()
    {
        instance = this;

        playerPrefabs = PlayerData.playerTeam.ToArray();
        enemyPrefabs = BattleData.enemyTeam;
    }

    private void Start()
    {
        battleState = BattleState.START;
        
        StartCoroutine(SetupBattleField());
    }

    IEnumerator SetupBattleField()
    {
        yield return null;

        playerUnits = new List<Unit>();
        enemyUnits = new List<Unit>();
        allUnits = new List<Unit>();

        //Instanciar unidades
        for (int i = 0; i < playerPrefabs.Length; i++)
        {
            //Calculo del offset en relacion a la cantidad de unidades
            Vector3 offset = new Vector3(4 * (i - (playerPrefabs.Length - 1) / 2f), 0, 0);
            playerUnits.Add(Instantiate(playerPrefabs[i], playerSide.position + offset, Quaternion.LookRotation(enemySide.position - playerSide.position - offset)).GetComponent<Unit>());
            allUnits.Add(playerUnits[i]);
            playerUnits[i].isPlayerControlled = true;
            playerUnits[i].id = i;
        }

        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            //Calculo del offset en relacion a la cantidad de unidades
            Vector3 offset = new Vector3(4 * (i - (enemyPrefabs.Length - 1) / 2f), 0, 0);
            enemyUnits.Add(Instantiate(enemyPrefabs[i], enemySide.position + offset, enemySide.rotation).GetComponent<Unit>());
            allUnits.Add(enemyUnits[i]);
        }

        yield return new WaitForSeconds(2);

        foreach (var unit in allUnits)
        {
            unit.OnBattleStart();
        }

        CalculateTurnOrder(allUnits);
        currentTurnIndex = 0;

        yield return new WaitForSeconds(.2f);

        StartNextTurn();
    }

    public void StartNextTurn(bool activateTurnStartEffect = true)
    {
        if (CheckUnitsWaitingForDestroy())
        {
            StartCoroutine(DelayedStartTurn(activateTurnStartEffect));
            return;
        }
        CameraManager.instance.ActivateMainCamera();

        CameraManager.instance.SetBlendTime(1);
        // Detener la corrutina anterior si existe
        if (currentTurnCoroutine != null)
            StopCoroutine(currentTurnCoroutine);

        // Verificar si hay unidades vivas
        if (IsBattleOver())
        {
            if (IsBattleWon())
            {
                Debug.Log("Win");
                battleState = BattleState.WIN;
                Debug.Log(playerUnits.Count);
                TeamManager.instance.SaveTeamData(playerUnits);
                StartCoroutine(EndBattle());
                return;
            }
            else
            {
                Debug.Log("Game over");
                battleState = BattleState.LOSS;
                StartCoroutine(EndBattle());
                return;
            }
        }

        if (currentTurnIndex < allUnits.Count)
        {
            Unit currentUnit = allUnits[currentTurnIndex];
            if (currentUnit.waitingForDestroy)
            {
                currentTurnIndex++;
                StartNextTurn();
                return;
            }
            Debug.Log("Iniciando turno de: " + currentUnit.name + " (Índice: " + currentTurnIndex + ")");

            if (playerUnits.Contains(currentUnit))
                currentTurnCoroutine = StartCoroutine(PlayerTurn(currentUnit, activateTurnStartEffect));
            else
                currentTurnCoroutine = StartCoroutine(EnemyTurn(currentUnit, activateTurnStartEffect));
        }
        else
        {
            // Fin de la ronda, recalcular orden y empezar de nuevo
            Debug.Log("Round " + round + " Ended");
            round++;
            currentTurnIndex = 0;

            StopAllCoroutines();
            // Recalcular orden basado en speed
            CalculateTurnOrder(allUnits);

            // Empezar nueva ronda
            StartNextTurn();
        }
    }

    IEnumerator EndBattle()
    {
        yield return new WaitForSeconds(2f);
        MapManager.instance.LoadMapScene();
    }

    bool CheckUnitsWaitingForDestroy()
    {
        foreach (Unit unit in allUnits)
        {
            if (unit.waitingForDestroy)
            {
                return true;
            }
        }

        return false;
    }
    IEnumerator DelayedStartTurn(bool activateTurnStartEffect = true)
    {
        yield return new WaitForSeconds(.1f);
        StartNextTurn(activateTurnStartEffect);
    }

    IEnumerator PlayerTurn(Unit currentUnit, bool activateTurnStartEffect = true)
    {
        battleState = BattleState.PLAYERTURN;
        currentUnit.ActivateCamera();
        if(activateTurnStartEffect) currentUnit.OnTurnStart();
        if (currentUnit.skipTurn)
        {
            currentUnit.OnTurnEnd();
            yield return new WaitForSeconds(1);
            currentUnit.DeactivateCamera();
            currentUnit.skipTurn = false;
            currentTurnIndex++;
            StartNextTurn();
            yield break;
        }
        else
        {
            currentUnit.OpenBattleMenu();

            yield return null;
        }
    }

    IEnumerator EnemyTurn(Unit currentUnit, bool activateTurnStartEffect = true)
    {
        battleState = BattleState.ENEMYTURN;
        if (activateTurnStartEffect) currentUnit.OnTurnStart();
        yield return new WaitForSeconds(2);

        List<Abilities> usableAbilities = new List<Abilities>();

        for (int i = 0; i < currentUnit.knownAbilities.Length; i++)
        {
            if (currentUnit.knownAbilities[i].abilityType == AbilityType.ACTIVE)
            {
                if(currentUnit.knownAbilities[i].mustUseStance && 
                    currentUnit.currentStance == currentUnit.knownAbilities[i].stance)
                {
                    usableAbilities.Add(currentUnit.knownAbilities[i]);
                }
                else if (!currentUnit.knownAbilities[i].mustUseStance)
                {
                    usableAbilities.Add(currentUnit.knownAbilities[i]);
                }
            }
        }

        Abilities chosenAbility = usableAbilities[UnityEngine.Random.Range(0, usableAbilities.Count)];

        switch (chosenAbility.target)
        {
            case AbilityTarget.SELF:
                yield return StartCoroutine(AttackSequence(currentUnit, currentUnit, chosenAbility));
                break;
            case AbilityTarget.ONEENEMY:
                yield return StartCoroutine(AttackSequence(currentUnit, playerUnits[UnityEngine.Random.Range(0, playerUnits.Count)], chosenAbility));
                break;
            case AbilityTarget.ONEALLY:
                yield return StartCoroutine(AttackSequence(currentUnit, enemyUnits[UnityEngine.Random.Range(0, enemyUnits.Count)], chosenAbility));
                break;
            case AbilityTarget.ALLENEMIES:
                StartCoroutine(AttackSequence(currentUnit, playerUnits.ToArray(), chosenAbility));
                break;
            case AbilityTarget.ALLALLIES:
                StartCoroutine(AttackSequence(currentUnit, enemyUnits.ToArray(), chosenAbility));
                break;
            case AbilityTarget.ALL:
                StartCoroutine(AttackSequence(currentUnit, allUnits.ToArray(), chosenAbility));
                break;
            default:
                break;
        }
    }

    public void AbilityMenu(Unit attacker) //Se llama desde la interfaz del jugador, los botones se suscriben al activarse
    {
        attacker.ActivateCamera();
        attacker.CloseBattleMenu();
        attacker.OpenAbilityMenu();
    }

    public void SelectAbility(Abilities ability)
    {
        StartCoroutine(ActivateAbility(ability));
    }
    public IEnumerator ActivateAbility(Abilities ability)
    {
        Unit currentUnit = allUnits[currentTurnIndex];
        CameraManager.instance.SetBlendTime(2f);
        currentUnit.CloseAbilityMenu();
        currentUnit.DeactivateCamera();

        bool confirm = false;
        int selection = 0;

        switch (ability.target)
        {
            case GameData.AbilityTarget.SELF:
                currentUnit.SelectTarget(currentUnit.gameObject);
                yield return Run<bool>(WaitForConfirm(), (output) => confirm = output);

                if (!confirm)
                {
                    AbilityMenu(currentUnit);
                    yield break;
                }
                else StartCoroutine(AttackSequence(currentUnit, currentUnit, ability));

                yield break;

            case GameData.AbilityTarget.ONEENEMY:
                yield return Run<int>(SelectTarget(), (output) => selection = output);
                if(selection >= 0) StartCoroutine(AttackSequence(currentUnit, enemyUnits[selection], ability));
                yield break;

            case GameData.AbilityTarget.ONEALLY:
                yield return Run<int>(SelectTarget(false), (output) => selection = output);
                if (selection >= 0) StartCoroutine(AttackSequence(currentUnit, playerUnits[selection], ability));
                yield break;

            case GameData.AbilityTarget.ALLENEMIES:
                currentUnit.SelectTarget(enemySide.gameObject);
                yield return Run<bool>(WaitForConfirm(), (output) => confirm = output);
                if (!confirm)
                {
                    AbilityMenu(currentUnit);
                    yield break;
                }
                else StartCoroutine(AttackSequence(currentUnit, enemyUnits.ToArray(), ability));
                yield break;

            case GameData.AbilityTarget.ALLALLIES:
                currentUnit.SelectTarget(playerSide.gameObject);
                yield return Run<bool>(WaitForConfirm(), (output) => confirm = output);
                if (!confirm)
                {
                    AbilityMenu(currentUnit);
                    yield break;
                }
                else StartCoroutine(AttackSequence(currentUnit, playerUnits.ToArray(), ability));
                yield break;

            case GameData.AbilityTarget.ALL:
                CameraManager.instance.SetBlendTime(.75f);
                CameraManager.instance.ActivateMainCamera();
                yield return Run<bool>(WaitForConfirm(), (output) => confirm = output);
                if (!confirm)
                {
                    AbilityMenu(currentUnit);
                    yield break;
                }
                else StartCoroutine(AttackSequence(currentUnit, allUnits.ToArray(), ability));
                yield break;

            default:
                yield break;
        }
    }

    IEnumerator WaitForConfirm()
    {
        Unit currentUnit = allUnits[currentTurnIndex];
        bool result;
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                result = true;
                yield return result;
                yield break;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                result = false;
                yield return result;
                yield break;
            }

            yield return null;
        }
    }

    IEnumerator SelectTarget(bool enemySide = true)
    {
        int selection = 0;
        Unit attacker = allUnits[currentTurnIndex];
        if (enemySide)
        {
            attacker.SelectTarget(enemyUnits[selection].gameObject);

            while (true)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    if (selection == 0)
                    {
                        selection = enemyUnits.Count - 1;
                    }
                    else
                    {
                        selection--;
                    }

                    attacker.SelectTarget(enemyUnits[selection].gameObject);
                }

                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    if (selection == enemyUnits.Count - 1)
                    {
                        selection = 0;
                    }
                    else
                    {
                        selection++;
                    }

                    attacker.SelectTarget(enemyUnits[selection].gameObject);
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    yield return selection;
                    yield break;
                }

                if(Input.GetKeyDown(KeyCode.Escape))
                {
                    AbilityMenu(attacker);
                    selection = -1;
                    yield return selection;
                    yield break;
                }

                yield return null;
            }
        }
        else
        {
            attacker.SelectTarget(playerUnits[selection].gameObject);

            while (true)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    if (selection == 0)
                    {
                        selection = playerUnits.Count - 1;
                    }
                    else
                    {
                        selection--;
                    }

                    attacker.SelectTarget(playerUnits[selection].gameObject);
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    if (selection == playerUnits.Count - 1)
                    {
                        selection = 0;
                    }
                    else
                    {
                        selection++;
                    }

                    attacker.SelectTarget(playerUnits[selection].gameObject);
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    yield return selection;
                    yield break;
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    AbilityMenu(attacker);
                    selection = -1;
                    yield return selection;
                    yield break;
                }

                yield return null;
            }
        }
    }
    public void Death(Unit attacker)
    {
        if (allUnits.FindIndex(x => x.Equals(attacker)) < currentTurnIndex) currentTurnIndex--;

        allUnits.Remove(attacker);
        if(enemyUnits.Contains(attacker)) enemyUnits.Remove(attacker);
        else playerUnits.Remove(attacker);

        Destroy(attacker.gameObject);
    }
    public void Run(Unit attacker) //Se llama desde la interfaz del jugador, los botones se suscriben al activarse
    {
        attacker.DeactivateCamera();
        attacker.CloseBattleMenu();

        allUnits.Remove(attacker);
        playerUnits.Remove(attacker);

        StartCoroutine(RunSequence(attacker));
    }

    IEnumerator RunSequence(Unit attacker)
    {
        CameraManager.instance.ActivateAttackCamera();

        yield return new WaitForSeconds(1);

        Destroy(attacker.gameObject);

        yield return new WaitForSeconds(1);

        CameraManager.instance.ActivateMainCamera();

        // Avanzar al siguiente turno
        StartNextTurn();
    }
    IEnumerator AttackSequence(Unit attacker, Unit[] targets, Abilities ability)
    {
        Transform visualTarget = playerUnits.Contains(targets[0]) ? playerSide : enemySide;
        attacker.EndSelect();
        CameraManager.instance.ActivateAttackCamera();

        Debug.Log("Atacando con: " + attacker.name);

        Vector3 attackerStartPos = attacker.transform.position;
        float t = 0;
        float elapsedTime = 0;

        yield return new WaitForSeconds(.2f);

        if (ability.vfxPrefab)
        {
            if (ability.spawnVfxOnSelf)
            {
                Vector3 dir = visualTarget.position - attackerStartPos;
                GameObject vfx = Instantiate(ability.vfxPrefab, attackerStartPos + .1f * dir, Quaternion.LookRotation(dir));
                yield return new WaitForSeconds(vfx.GetComponent<ParticleSystem>().main.duration/2);
                Damage(ability, attacker, targets);
                yield return new WaitForSeconds(vfx.GetComponent<ParticleSystem>().main.duration / 2);
                Destroy(vfx);
            }
            else
            {
                GameObject vfx = Instantiate(ability.vfxPrefab, visualTarget);
                yield return new WaitForSeconds(vfx.GetComponent<ParticleSystem>().main.duration / 2);
                Damage(ability, attacker, targets);
                yield return new WaitForSeconds(vfx.GetComponent<ParticleSystem>().main.duration / 2);
                Destroy(vfx);
            }
        }
        else
        {
            //Se abalanza el personaje (ida)
            while (t < .8f)
            {
                elapsedTime += Time.deltaTime;
                t += elapsedTime * elapsedTime / 10;
                attacker.transform.position = Vector3.Lerp(attackerStartPos, visualTarget.transform.position, t);
                yield return null;
            }

            Vector3 attackerEndPos = attacker.transform.position;
            Damage(ability, attacker, targets);
            t = 0;
            yield return new WaitForSeconds(0.1f); // Pequeña pausa en el impacto

            // Regreso
            while (t < 1)
            {
                t += Time.deltaTime;
                attacker.transform.position = Vector3.Lerp(attackerEndPos, attackerStartPos, t);
                yield return null;
            }

            // Asegurar que regresó a su posición exacta
            attacker.transform.position = attackerStartPos;
        }


        

        CameraManager.instance.SetBlendTime(1);

        
        // Importante: Esperar un frame antes de activar la cámara principal
        yield return new WaitForSeconds(0.3f);

        if (attacker.HasAdditionalTurn())
        {
            StartNextTurn(false);
            yield break;
        }
        else
        {
            attacker.OnTurnEnd();
            currentTurnIndex++;
            // Avanzar al siguiente turno
            StartNextTurn();
            yield break;
        }
        
    }

    IEnumerator AttackSequence(Unit attacker, Unit target, Abilities ability)
    {
        attacker.EndSelect();
        CameraManager.instance.ActivateAttackCamera();

        Vector3 attackerStartPos = attacker.transform.position;
        float t = 0;
        float elapsedTime = 0;

        Unit[] targets = new Unit[1];
        targets[0] = target;

        yield return new WaitForSeconds(.2f);

        if (ability.vfxPrefab)
        {
            if (ability.spawnVfxOnSelf)
            {
                Vector3 dir = target.transform.position - attackerStartPos;
                GameObject vfx = Instantiate(ability.vfxPrefab, attackerStartPos + .1f * dir, Quaternion.LookRotation(dir));
                yield return new WaitForSeconds(vfx.GetComponent<ParticleSystem>().main.duration / 2);
                Damage(ability, attacker, targets);
                yield return new WaitForSeconds(vfx.GetComponent<ParticleSystem>().main.duration / 2);
                Destroy(vfx);
            }
            else
            {
                GameObject vfx = Instantiate(ability.vfxPrefab, target.transform);
                yield return new WaitForSeconds(vfx.GetComponent<ParticleSystem>().main.duration / 2);
                Damage(ability, attacker, targets);
                yield return new WaitForSeconds(vfx.GetComponent<ParticleSystem>().main.duration / 2);
                Destroy(vfx);
            }
        }
        else 
        {
            //Se abalanza el personaje (ida)
            while (t < .8f)
            {
                elapsedTime += Time.deltaTime;
                t += elapsedTime * elapsedTime / 10;
                attacker.transform.position = Vector3.Lerp(attackerStartPos, target.transform.position, t);
                yield return null;
            }

            Vector3 attackerEndPos = attacker.transform.position;
            Damage(ability, attacker, targets);

            t = 0;
            yield return new WaitForSeconds(0.1f); // Pequeña pausa en el impacto

            // Regreso
            while (t < 1)
            {
                t += Time.deltaTime;
                attacker.transform.position = Vector3.Lerp(attackerEndPos, attackerStartPos, t);
                yield return null;
            }

            // Asegurar que regresó a su posición exacta
            attacker.transform.position = attackerStartPos;
        }

        


        CameraManager.instance.SetBlendTime(1);

        
        // Importante: Esperar un frame antes de activar la cámara principal
        yield return new WaitForSeconds(0.3f);

        if (attacker.HasAdditionalTurn())
        {
            StartNextTurn(false);
            yield break;
        }
        else
        {
            attacker.OnTurnEnd();
            currentTurnIndex++;
            // Avanzar al siguiente turno
            StartNextTurn();
            yield break;
        }
    }

    void Damage(Abilities ability, Unit attacker, Unit[] targets)
    {
        //Check if attack hit
        if (!CheckHit(ability))
        {
            Debug.Log(attacker.name + "missed");
        }
        else
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (ability.power != 0)
                {
                    targets[i].TakeDamage(CalculateAttackDamage(attacker, targets[i], ability));
                    attacker.ResolvePassiveEffect(PassiveExecutionTime.ONHIT, targets[i]);
                }

                ResolveAbilityEffect(attacker, targets[i], ability, ability.effect1, ability.effect1Chance, ability.affectSelf);
                ResolveAbilityEffect(attacker, targets[i], ability, ability.effect2, ability.effect2Chance, ability.affectSelf);
            }
        }
    }
    void ResolveAbilityEffect(Unit attacker, Unit target, Abilities ability, AbilityEffect effect, float effectChance, bool affectSelf)
    {
        if (effectChance >= UnityEngine.Random.Range(1, 101))
        {
            switch (effect)
            {
                case GameData.AbilityEffect.NONE:
                    break;
                case GameData.AbilityEffect.HEAL:
                    /*to do*/
                    break;
                case GameData.AbilityEffect.UPATK:
                    if(affectSelf) attacker.ApplyStatModifier(Stats.ATK, 1.5f);
                    else target.ApplyStatModifier(Stats.ATK, 1.5f);
                    break;
                case GameData.AbilityEffect.UPDEF:
                    if (affectSelf) attacker.ApplyStatModifier(Stats.DEF, 1.5f);
                    else target.ApplyStatModifier(Stats.DEF, 1.5f);
                    break;
                case GameData.AbilityEffect.UPSPEED:
                    if (affectSelf) attacker.ApplyStatModifier(Stats.SPEED, 1.5f);
                    else target.ApplyStatModifier(Stats.SPEED, 1.5f);
                    break;
                case GameData.AbilityEffect.DOWNATK:
                    if (affectSelf) attacker.ApplyStatModifier(Stats.ATK, .75f);
                    else target.ApplyStatModifier(Stats.ATK, .75f);
                    break;
                case GameData.AbilityEffect.DOWNDEF:
                    if (affectSelf) attacker.ApplyStatModifier(Stats.DEF, .75f);
                    else target.ApplyStatModifier(Stats.DEF, .75f);
                    break;
                case GameData.AbilityEffect.DOWNSPEED:
                    if (affectSelf) attacker.ApplyStatModifier(Stats.SPEED, .75f);
                    else target.ApplyStatModifier(Stats.SPEED, .75f);
                    break;
                case GameData.AbilityEffect.STANCECHANGE:
                    if(affectSelf) attacker.currentStance = ability.stanceToChangeTo;
                    else target.currentStance = ability.stanceToChangeTo;
                    break;
                case GameData.AbilityEffect.APPLYSTATUS:
                    if (affectSelf) attacker.ApplyStatus(ability.status);
                    else target.ApplyStatus(ability.status);
                    break;
                case AbilityEffect.CURESTATUS:
                    target.CureStatus();
                    break;
                default:
                    break;
            }
        }
    }
    bool CheckHit(Abilities ability)
    {
        if (ability.accuracy >= UnityEngine.Random.Range(1,100)) return true;
        else return false;
    }
    int CalculateAttackDamage(Unit attacker, Unit target, Abilities ability)
    {
        int attackStat = attacker.GetStat(Stats.ATK);
        int defenseStat = target.GetStat(Stats.DEF);
        float stanceBonus = attacker.currentStance == ability.stance ? 1.5f : 1;
        float efficacy = GetAbilityEfficacy(ability.stance, target.currentStance);
        float roll = UnityEngine.Random.Range(.8f, 1f);
        bool isCritical = UnityEngine.Random.Range(0, 16) == 0;
        float critMod = isCritical ? 1.5f : 1f;

        int damage = Mathf.FloorToInt((((2 * attacker.level + 2) * .1f * ability.power * attackStat / (5 * defenseStat)) + 2) * efficacy * stanceBonus * roll * critMod);

        Debug.Log(attacker.name + " attacks " + target.name + " dealing: " + damage + " damage.");
        if (efficacy == 2) Debug.Log("It's super effective!");
        if (isCritical) Debug.Log("Critical Hit!");

        return damage;
    }

    private void CalculateTurnOrder(List<Unit> allUnits)
    {
        allUnits.Sort(delegate (Unit x, Unit y)
        {
            return y.GetStat(Stats.SPEED).CompareTo(x.GetStat(Stats.SPEED));
        });
    }

    public static IEnumerator Run<T>(IEnumerator target, Action<T> output)
    {
        object result = null;
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
        output((T)result);
    }

    bool IsBattleOver()
    {
        if (enemyUnits.Count == 0) return true;
        if (playerUnits.Count == 0) return true;

        return false;
    }

    bool IsBattleWon()
    {
        if (enemyUnits.Count == 0) return true;
        return false;
    }

    float GetAbilityEfficacy(Stance abilityStance, Stance defenderStance)
    {
        switch (abilityStance)
        {
            case Stance.AGRESSIVE:
                if (defenderStance == Stance.AGILE) return 2f;
                else return 1;
            case Stance.DEFENSIVE:
                if (defenderStance == Stance.AGRESSIVE) return 2f;
                else return 1;
            case Stance.AGILE:
                if (defenderStance == Stance.DEFENSIVE) return 2f;
                else return 1;
            case Stance.CAUTIOUS:
                return 1;
            case Stance.TRICKY:
                return 1;
            default:
                return 1;
        }
    }
}