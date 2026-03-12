using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using System;
using GameData;


//Turn Based Battle System
public enum BattleState { START, PLAYERTURN, ENEMYTURN, LOSS, WIN }
public class TBBS : MonoBehaviour
{
    public static TBBS instance;
    public GameObject[] playerPrefabs;
    public GameObject[] enemyPrefabs;

    public Transform playerSide;
    public Transform enemySide;

    private List<Unit> playerUnits;
    private List<Unit> enemyUnits;

    private BattleState battleState;
    private int currentTurnIndex = 0;
    private int round = 0;

    private List<Unit> allUnits;

    private Coroutine currentTurnCoroutine;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        battleState = BattleState.START;
        StartCoroutine(SetupBattleField());
    }

    IEnumerator SetupBattleField()
    {
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
        }

        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            //Calculo del offset en relacion a la cantidad de unidades
            Vector3 offset = new Vector3(4 * (i - (enemyPrefabs.Length - 1) / 2f), 0, 0);
            enemyUnits.Add(Instantiate(enemyPrefabs[i], enemySide.position + offset, enemySide.rotation).GetComponent<Unit>());
            allUnits.Add(enemyUnits[i]);
        }

        yield return new WaitForNextFrameUnit();

        CalculateTurnOrder(allUnits);
        currentTurnIndex = 0;

        yield return new WaitForSeconds(2);

        StartNextTurn();
    }

    void StartNextTurn()
    {
        CameraManager.instance.SetBlendTime(1);
        // Detener la corrutina anterior si existe
        if (currentTurnCoroutine != null)
            StopCoroutine(currentTurnCoroutine);

        // Verificar si hay unidades vivas
        if (enemyUnits.Count <= 0)
        {
            Debug.Log("Win");
            battleState = BattleState.WIN;
            return;
        }
        if (playerUnits.Count <= 0)
        {
            Debug.Log("Game over");
            battleState = BattleState.LOSS;
            return;
        }

        if (currentTurnIndex < allUnits.Count)
        {
            Unit currentUnit = allUnits[currentTurnIndex];
            Debug.Log("Iniciando turno de: " + currentUnit.name + " (Índice: " + currentTurnIndex + ")");

            if (playerUnits.Contains(currentUnit))
                currentTurnCoroutine = StartCoroutine(PlayerTurn(currentUnit));
            else
                currentTurnCoroutine = StartCoroutine(EnemyTurn(currentUnit));
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

    IEnumerator PlayerTurn(Unit currentUnit)
    {
        battleState = BattleState.PLAYERTURN;
        currentUnit.ActivateCamera();

        currentUnit.OpenBattleMenu();

        yield return null;
    }

    IEnumerator EnemyTurn(Unit currentUnit)
    {
        battleState = BattleState.ENEMYTURN;
        yield return new WaitForSeconds(2);

        // Ejecutar el ataque y esperar a que termine

        yield return StartCoroutine(AttackSequence(currentUnit, playerUnits[UnityEngine.Random.Range(0, playerUnits.Count)], currentUnit.knownAbilities[0]));
    }

    public void AbilityMenu(Unit attacker) //Se llama desde la interfaz del jugador, los botones se suscriben al activarse
    {
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

                if (!confirm) StartCoroutine(PlayerTurn(currentUnit));
                else StartCoroutine(AttackSequence(currentUnit, currentUnit, ability));

                break;

            case GameData.AbilityTarget.ONEENEMY:
                yield return Run<int>(SelectTarget(currentUnit), (output) => selection = output);
                StartCoroutine(AttackSequence(currentUnit, enemyUnits[selection], ability));
                break;

            case GameData.AbilityTarget.ONEALLY:
                yield return Run<int>(SelectTarget(currentUnit, false), (output) => selection = output);
                StartCoroutine(AttackSequence(currentUnit, playerUnits[selection], ability));
                break;

            case GameData.AbilityTarget.ALLENEMIES:
                currentUnit.SelectTarget(enemySide.gameObject);
                yield return Run<bool>(WaitForConfirm(), (output) => confirm = output);
                if (!confirm) StartCoroutine(PlayerTurn(currentUnit));
                else StartCoroutine(AttackSequence(currentUnit, enemyUnits.ToArray(), ability));
                break;

            case GameData.AbilityTarget.ALLALLIES:
                currentUnit.SelectTarget(playerSide.gameObject);
                yield return Run<bool>(WaitForConfirm(), (output) => confirm = output);
                if (!confirm) StartCoroutine(PlayerTurn(currentUnit));
                else StartCoroutine(AttackSequence(currentUnit, playerUnits.ToArray(), ability));
                break;

            case GameData.AbilityTarget.ALL:
                CameraManager.instance.SetBlendTime(.75f);
                CameraManager.instance.ActivateMainCamera();
                yield return Run<bool>(WaitForConfirm(), (output) => confirm = output);
                if (!confirm) StartCoroutine(PlayerTurn(currentUnit));
                else StartCoroutine(AttackSequence(currentUnit, allUnits.ToArray(), ability));
                break;

            default:
                break;
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
                break;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                result = false;
                yield return result;
                break;
            }

            yield return null;
        }
    }

    IEnumerator SelectTarget(Unit attacker, bool enemySide = true)
    {
        int selection = 0;

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

                yield return selection;
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

                yield return selection;
            }
        }
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
        Unit visualTarget = targets[0];
        attacker.EndSelect();
        CameraManager.instance.ActivateAttackCamera();

        Debug.Log("Atacando con: " + attacker.name);

        Vector3 attackerStartPos = attacker.transform.position;
        float t = 0;
        float elapsedTime = 0;

        yield return new WaitForSeconds(.2f);

        //Se abalanza el personaje (ida)
        while (t < .8f)
        {
            elapsedTime += Time.deltaTime;
            t += elapsedTime * elapsedTime / 10;
            attacker.transform.position = Vector3.Lerp(attackerStartPos, visualTarget.transform.position, t);
            yield return null;
        }
        //Check if attack hit
        if (!CheckHit(ability))
        {
            Debug.Log(attacker.name + "missed");
        }
        else
        {
            Debug.Log(attacker.name + " attacks " + visualTarget.name + " dealing: " + CalculateAttackDamage(attacker, visualTarget) + " damage.");
            ResolveAbilityEffect(attacker, targets, ability.effect1, ability.effect1Chance);
            ResolveAbilityEffect(attacker, targets, ability.effect2, ability.effect2Chance);

        }

        Vector3 attackerEndPos = attacker.transform.position;

        t = 0;
        yield return new WaitForSeconds(0.1f); // Pequeña pausa en el impacto

        // Regreso
        while (t < 1)
        {
            t += Time.deltaTime;
            attacker.transform.position = Vector3.Lerp(attackerEndPos, attackerStartPos, t);
            yield return null;
        }

        CameraManager.instance.SetBlendTime(1);

        // Asegurar que regresó a su posición exacta
        attacker.transform.position = attackerStartPos;

        attacker.OnTurnEnd();
        // Importante: Esperar un frame antes de activar la cámara principal
        yield return null;

        CameraManager.instance.ActivateMainCamera();

        currentTurnIndex++;
        // Avanzar al siguiente turno
        StartNextTurn();
    }

    IEnumerator AttackSequence(Unit attacker, Unit target, Abilities ability)
    {
        Unit visualTarget = target;
        attacker.EndSelect();
        CameraManager.instance.ActivateAttackCamera();

        Debug.Log("Atacando con: " + attacker.name);

        Vector3 attackerStartPos = attacker.transform.position;
        float t = 0;
        float elapsedTime = 0;

        yield return new WaitForSeconds(.2f);

        //Se abalanza el personaje (ida)
        while (t < .8f)
        {
            elapsedTime += Time.deltaTime;
            t += elapsedTime * elapsedTime / 10;
            attacker.transform.position = Vector3.Lerp(attackerStartPos, visualTarget.transform.position, t);
            yield return null;
        }
        //Check if attack hit
        if (!CheckHit(ability))
        {
            Debug.Log(attacker.name + "missed");
        }
        else
        {
            Debug.Log(attacker.name + " attacks " + visualTarget.name + " dealing: " + CalculateAttackDamage(attacker, visualTarget) + " damage.");
            ResolveAbilityEffect(attacker, target, ability.effect1, ability.effect1Chance);
            ResolveAbilityEffect(attacker, target, ability.effect2, ability.effect2Chance);

        }
        Vector3 attackerEndPos = attacker.transform.position;

        t = 0;
        yield return new WaitForSeconds(0.1f); // Pequeña pausa en el impacto

        // Regreso
        while (t < 1)
        {
            t += Time.deltaTime;
            attacker.transform.position = Vector3.Lerp(attackerEndPos, attackerStartPos, t);
            yield return null;
        }

        CameraManager.instance.SetBlendTime(1);

        // Asegurar que regresó a su posición exacta
        attacker.transform.position = attackerStartPos;

        attacker.OnTurnEnd();
        // Importante: Esperar un frame antes de activar la cámara principal
        yield return null;

        CameraManager.instance.ActivateMainCamera();

        currentTurnIndex++;
        // Avanzar al siguiente turno
        StartNextTurn();
    }

    void ResolveAbilityEffect(Unit attacker, Unit[] targets, AbilityEffect effect, float effectChance)
    {
        if (effectChance >= UnityEngine.Random.Range(0, 100))
        {
            switch (effect)
            {
                case GameData.AbilityEffect.NONE:
                    break;
                case GameData.AbilityEffect.HEAL:
                    foreach (var item in targets) { item.Heal(1f); }
                    break;
                case GameData.AbilityEffect.UPATK:
                    foreach (var item in targets) { item.ApplyStatModifier(Stats.ATK, 1.5f); }
                    break;
                case GameData.AbilityEffect.UPDEF:
                    foreach (var item in targets) { item.ApplyStatModifier(Stats.DEF, 1.5f); }
                    break;
                case GameData.AbilityEffect.UPSPEED:
                    foreach (var item in targets) { item.ApplyStatModifier(Stats.SPEED, 1.5f); }
                    break;
                case GameData.AbilityEffect.DOWNATK:
                    foreach (var item in targets) { item.ApplyStatModifier(Stats.ATK, .75f); }
                    break;
                case GameData.AbilityEffect.DOWNDEF:
                    foreach (var item in targets) { item.ApplyStatModifier(Stats.DEF, .75f); }
                    break;
                case GameData.AbilityEffect.DOWNSPEED:
                    foreach (var item in targets) { item.ApplyStatModifier(Stats.SPEED, .75f); }
                    break;
                case GameData.AbilityEffect.STANCECHANGE:
                    foreach (var item in targets) { /*to do*/ }
                    break;
                case GameData.AbilityEffect.APPLYBURN:
                    foreach (var item in targets) { item.status = Status.BURNED; }
                    break;
                case GameData.AbilityEffect.APPLYPARA:
                    foreach (var item in targets) { item.status = Status.PARALYZED; }
                    break;
                case GameData.AbilityEffect.APPLYPOISON:
                    foreach (var item in targets) { item.status = Status.POISONED; }
                    break;
                case GameData.AbilityEffect.APPLYFRZ:
                    foreach (var item in targets) { item.status = Status.FROZEN; }
                    break;
                case GameData.AbilityEffect.APPLYSLP:
                    foreach (var item in targets) { item.status = Status.ASLEEP; }
                    break;
                default:
                    break;
            }
        }
    }
    void ResolveAbilityEffect(Unit attacker, Unit target, AbilityEffect effect, float effectChance)
    {
        if (effectChance >= UnityEngine.Random.Range(0, 100))
        {
            switch (effect)
            {
                case GameData.AbilityEffect.NONE:
                    break;
                case GameData.AbilityEffect.HEAL:
                    /*to do*/
                    break;
                case GameData.AbilityEffect.UPATK:
                    target.ApplyStatModifier(Stats.ATK, 1.5f);
                    break;
                case GameData.AbilityEffect.UPDEF:
                    target.ApplyStatModifier(Stats.DEF, 1.5f);
                    break;
                case GameData.AbilityEffect.UPSPEED:
                    target.ApplyStatModifier(Stats.SPEED, 1.5f);
                    break;
                case GameData.AbilityEffect.DOWNATK:
                    target.ApplyStatModifier(Stats.ATK, .75f);
                    break;
                case GameData.AbilityEffect.DOWNDEF:
                    target.ApplyStatModifier(Stats.DEF, .75f);
                    break;
                case GameData.AbilityEffect.DOWNSPEED:
                    target.ApplyStatModifier(Stats.SPEED, .75f);
                    break;
                case GameData.AbilityEffect.STANCECHANGE:
                    /*to do*/
                    break;
                case GameData.AbilityEffect.APPLYBURN:
                    target.status = Status.BURNED;
                    break;
                case GameData.AbilityEffect.APPLYPARA:
                    target.status = Status.PARALYZED;
                    break;
                case GameData.AbilityEffect.APPLYPOISON:
                    target.status = Status.POISONED;
                    break;
                case GameData.AbilityEffect.APPLYFRZ:
                    target.status = Status.FROZEN;
                    break;
                case GameData.AbilityEffect.APPLYSLP:
                    target.status = Status.ASLEEP;
                    break;
                default:
                    break;
            }
        }
    }
    bool CheckHit(Abilities ability)
    {
        if (ability.accuracy >= UnityEngine.Random.Range(0,100)) return true;
        else return false;
    }
    int CalculateAttackDamage(Unit attacker, Unit target)
    {
        int attackStat = attacker.GetAttackStat();
        int defenseStat = target.GetDefenseStat();

        float roll = UnityEngine.Random.Range(.8f, 1f);
        bool isCritical = UnityEngine.Random.Range(0, 16) == 0;

        return Mathf.FloorToInt(((2 * attacker.level + 2) * (attackStat / defenseStat) / 5 + 2) * roll);
    }

    private void CalculateTurnOrder(List<Unit> allUnits)
    {
        allUnits.Sort(delegate (Unit x, Unit y)
        {
            return y.GetSpeedStat().CompareTo(x.GetSpeedStat());
        });

        for (int i = 0; i < allUnits.Count; i++)
        {
            Debug.Log(allUnits[i].name + " position: " + i);
        }
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
}