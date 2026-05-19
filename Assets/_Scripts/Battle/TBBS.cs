using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using System;
using GameData;
using UnityEngine.SceneManagement;
using static UnityEditor.Rendering.InspectorCurveEditor;
using Random = UnityEngine.Random;
using UnityEngine.UIElements;


//Turn Based Battle System
public class TBBS : MonoBehaviour
{
    public static TBBS instance;
    public GameObject[] playerPrefabs;
    public GameObject[] enemyPrefabs;

    public Transform playerSide;
    public Transform enemySide;

    public List<Unit> playerUnits;
    public List<Unit> enemyUnits;
    public List<GameObject> capturableUnits;

    public int currentTurnIndex = 0;
    private int round = 0;

    public List<Unit> allUnits;

    private Coroutine currentTurnCoroutine;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        StartCoroutine(SetupBattleField());
    }

    IEnumerator SetupBattleField()
    {
        AudioManager.instance.PlayMusic(AudioLibrary.instance.combatMusic, 1.0f);

        playerPrefabs = PlayerData.Instance.GetTeamPrefabs();
        enemyPrefabs = BattleData.enemyTeam;

        yield return null;

        playerUnits = new List<Unit>();
        enemyUnits = new List<Unit>();
        capturableUnits = new List<GameObject>();
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
            enemyUnits.Add(Instantiate(enemyPrefabs[i], enemySide.position + offset, Quaternion.LookRotation(playerSide.position - enemySide.position - offset)).GetComponent<Unit>());
            capturableUnits.Add(enemyPrefabs[i]);
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
                Debug.Log(playerUnits.Count);
                TeamManager.instance.SaveTeamData(playerUnits);
                EndScreenManager.instance.ShowVictoryScreen(playerUnits.ToArray(), capturableUnits.ToArray(), BattleData.goldReward, BattleData.expReward);
                PlayerData.Instance.gold += BattleData.goldReward;
                //StartCoroutine(EndBattle());
                return;
            }
            else
            {
                Debug.Log("Game over");
                StartCoroutine(EndBattle());
                return;
            }
        }

        if (currentTurnIndex < allUnits.Count)
        {
            Unit currentUnit = allUnits[currentTurnIndex];
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
            for (int i = 0;i < allUnits.Count;i++)
            {
                allUnits[i].OnRoundEnd();
            }
            StopAllCoroutines();
            // Recalcular orden basado en speed
            CalculateTurnOrder(allUnits);

            // Empezar nueva ronda
            StartNextTurn();
        }
    }

    IEnumerator EndBattle()
    {
        TooltipUI.instance.HideTooltipText();
        yield return new WaitForSeconds(2f);
        SceneManager.LoadSceneAsync("Daycare");
    }

    IEnumerator PlayerTurn(Unit currentUnit, bool activateTurnStartEffect = true)
    {
        currentUnit.ActivateCamera();
        if(activateTurnStartEffect) currentUnit.OnTurnStart();
        if (currentUnit.skipTurn)
        {
            TooltipUI.instance.ShowTooltipText(currentUnit.name + " flinched");
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
        if (activateTurnStartEffect) currentUnit.OnTurnStart();
        if (currentUnit.skipTurn)
        {
            TooltipUI.instance.ShowTooltipText(currentUnit.name + " flinched");
            currentUnit.OnTurnEnd();
            yield return new WaitForSeconds(1);
            currentUnit.skipTurn = false;
            currentTurnIndex++;
            StartNextTurn();
            yield break;
        }

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
                Unit target;
                if(playerUnits.Find(unit => unit.provoking)) target = playerUnits.Find(unit => unit.provoking);
                else
                {
                    target = playerUnits[UnityEngine.Random.Range(0, playerUnits.Count)];
                    if(target.guardedBy) target = target.guardedBy;
                }
                yield return StartCoroutine(AttackSequence(currentUnit, target, chosenAbility));
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

    public void ItemMenu(Unit attacker) //Se llama desde la interfaz del jugador, los botones se suscriben al activarse
    {
        attacker.ActivateCamera();
        attacker.CloseBattleMenu();
        attacker.OpenItemMenu();
    }

    public void SelectAbility(Abilities ability)
    {
        StartCoroutine(ActivateAbility(ability));
    }

    public void SelectItem(Item item)
    {
        StartCoroutine(ActivateItem(item));
    }

    public IEnumerator ActivateItem(Item item)
    {
        Unit currentUnit = allUnits[currentTurnIndex];
        CameraManager.instance.SetBlendTime(2f);
        currentUnit.CloseItemMenu();
        currentUnit.DeactivateCamera();

        int selection = 0;

        yield return Run<int>(SelectTarget(false), (output) => selection = output);
        if (selection >= 0) StartCoroutine(UseItem(item, playerUnits[selection]));
        yield break;
    }

    public IEnumerator UseItem(Item item, Unit target)
    {
        Unit currentUnit = allUnits[currentTurnIndex];

        switch (item.effect)
        {
            case ItemEffects.UPATK:
                break;
            case ItemEffects.UPDEF:
                break;
            case ItemEffects.UPSPEED:
                break;
            case ItemEffects.ADDTURN:
                break;
            case ItemEffects.APPLYSTATUS:
                break;
            case ItemEffects.HEAL:
                target.Heal(item.healingAmount);
                break;
            default:
                break;
        }

        yield return new WaitForSeconds(0.3f);

        if (currentUnit.HasAdditionalTurn())
        {
            StartNextTurn(false);
            yield break;
        }
        else
        {
            currentUnit.OnTurnEnd();
            currentTurnIndex++;
            // Avanzar al siguiente turno
            StartNextTurn();
            yield break;
        }
    }
    public IEnumerator ActivateAbility(Abilities ability)
    {
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        Unit currentUnit = allUnits[currentTurnIndex];
        CameraManager.instance.SetBlendTime(2f);
        currentUnit.CloseAbilityMenu();
        currentUnit.DeactivateCamera();

        List<Unit> targets = new List<Unit>();
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
                if (selection >= 0) 
                {
                    if(currentUnit.HasPassive("Stubborn") && Random.Range(1, 101) <= 10)
                    {
                        TooltipUI.instance.ShowTooltipText($"{currentUnit.name}'s is being stubborn");
                        selection = Random.Range(0, enemyUnits.Count);
                    }
                    StartCoroutine(AttackSequence(currentUnit, enemyUnits[selection], ability));
                } 
                else AbilityMenu(currentUnit);
                yield break;

            case GameData.AbilityTarget.ONEALLY:
                targets = new List<Unit>(playerUnits);
                targets.Remove(currentUnit);
                yield return Run<int>(SelectTarget(false), (output) => selection = output);
                if (selection >= 0)
                {
                    if (currentUnit.HasPassive("Stubborn") && Random.Range(1, 101) <= 10)
                    {
                        TooltipUI.instance.ShowTooltipText($"{currentUnit.name}'s is being stubborn");
                        selection = Random.Range(0, targets.Count);
                    }
                    StartCoroutine(AttackSequence(currentUnit, targets[selection], ability));
                }
                else AbilityMenu(currentUnit);
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
                targets = new List<Unit>(allUnits);
                if (currentUnit.HasPassive("Empath")) targets = new List<Unit>(enemyUnits);
                else targets.Remove(currentUnit);
                CameraManager.instance.SetBlendTime(.75f);
                CameraManager.instance.ActivateMainCamera();
                yield return Run<bool>(WaitForConfirm(), (output) => confirm = output);
                if (!confirm)
                {
                    AbilityMenu(currentUnit);
                    yield break;
                }
                else StartCoroutine(AttackSequence(currentUnit, targets.ToArray(), ability));
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
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                result = true;
                yield return result;
                yield break;
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
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

        yield return null;

        if (enemySide)
        {
            attacker.SelectTarget(enemyUnits[selection].gameObject);

            while (true)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.mouseScrollDelta.sqrMagnitude < 0)
                {
                    if (selection == 0) selection = enemyUnits.Count - 1;
                    else selection--;
                    attacker.SelectTarget(enemyUnits[selection].gameObject);
                }

                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.mouseScrollDelta.sqrMagnitude > 0)
                {
                    if (selection == enemyUnits.Count - 1) selection = 0;
                    else selection++;
                    attacker.SelectTarget(enemyUnits[selection].gameObject);
                }

                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    yield return selection;
                    yield break;
                }

                if (Input.GetMouseButtonDown(1))
                {
                    selection = -1;
                    yield return selection;
                    yield break;
                }

                yield return null;
            }
        }
        else
        {
            List<Unit> targets = new List<Unit>(playerUnits);
            targets.Remove(attacker);
            if(targets.Count > 0) attacker.SelectTarget(targets[selection].gameObject);
            else
            {
                selection = -1;
                yield return selection;
                yield break;
            }

            while (true)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.mouseScrollDelta.sqrMagnitude > 0)
                {
                    if (selection == 0) selection = targets.Count - 1;
                    else selection--;
                    attacker.SelectTarget(targets[selection].gameObject);
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.mouseScrollDelta.sqrMagnitude < 0)
                {
                    if (selection == targets.Count - 1) selection = 0;
                    else selection++;
                    attacker.SelectTarget(targets[selection].gameObject);
                }

                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    yield return selection;
                    yield break;
                }

                if (Input.GetMouseButtonDown(1))
                {
                    selection = -1;
                    yield return selection;
                    yield break;
                }

                yield return null;
            }
        }
    }
    public void WaitingForDestroy(Unit attacker)
    {
        if (allUnits.FindIndex(x => x.Equals(attacker)) < currentTurnIndex) currentTurnIndex--;

        allUnits.Remove(attacker);
        if (enemyUnits.Contains(attacker)) enemyUnits.Remove(attacker);
        else
        {
            if(playerUnits.FindIndex(x => x.Equals(attacker)) >= 0) PlayerData.teamData.Remove(PlayerData.teamData[playerUnits.FindIndex(x => x.Equals(attacker))]);
            playerUnits.Remove(attacker);
        }
    }
    public void Skip(Unit attacker) //Se llama desde la interfaz del jugador, los botones se suscriben al activarse
    {
        attacker.DeactivateCamera();
        attacker.CloseBattleMenu();
        attacker.OnTurnEnd();
        currentTurnIndex++;
        StartNextTurn();
    }
    IEnumerator AttackSequence(Unit attacker, Unit[] targets, Abilities ability)
    {
        Transform visualTarget = playerUnits.Contains(targets[0]) ? playerSide : enemySide;
        attacker.EndSelect();
        CameraManager.instance.ActivateAttackCamera();

        TooltipUI.instance.ShowTooltipText(attacker.name + " uses " + ability.name);

        Vector3 attackerStartPos = attacker.transform.position;
        float t = 0;
        float elapsedTime = 0;

        yield return new WaitForSeconds(1f);

        int hits = 1;
        if(ability.multiHit) hits = ability.hits;
        else if(ability.multiHitRange)hits = Random.Range(ability.hitRange[0], ability.hitRange[1]);

        bool nextAttack = false;

        for (int i = 0; i < hits; i++)
        {
            if(i > 0)
            {
                List<Unit> newTargets = new List<Unit>();
                for (int j = 0; j < targets.Length; j++)
                {
                    if (targets[j] != null) newTargets.Add(targets[j]);
                }

                targets = newTargets.ToArray();
            }
            if (ability.vfxPrefab)
            {
                if (ability.spawnVfxOnSelf)
                {
                    Vector3 dir = visualTarget.position - attackerStartPos;
                    GameObject vfx = Instantiate(ability.vfxPrefab, attackerStartPos + .1f * dir, Quaternion.LookRotation(dir));
                    if (ability.sfx) AudioManager.instance.PlaySound3D(ability.sfx, attackerStartPos);
                    yield return new WaitForSeconds(vfx.GetComponent<ParticleSystem>().main.duration / 2);
                    nextAttack = Damage(ability, attacker, targets);
                    yield return new WaitForSeconds(vfx.GetComponent<ParticleSystem>().main.duration / 2);
                    Destroy(vfx);
                }
                else
                {
                    GameObject vfx = Instantiate(ability.vfxPrefab, visualTarget);
                    if (ability.sfx) AudioManager.instance.PlaySound3D(ability.sfx, visualTarget.position);
                    yield return new WaitForSeconds(vfx.GetComponent<ParticleSystem>().main.duration / 2);
                    nextAttack = Damage(ability, attacker, targets);
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
                if (ability.sfx) AudioManager.instance.PlaySound3D(ability.sfx, attackerEndPos);
                nextAttack = Damage(ability, attacker, targets);
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

                t = 0;

                if (!nextAttack) break;
                yield return new WaitForSeconds(0.1f);
            }
        }   

        CameraManager.instance.SetBlendTime(1);

        // Importante: Esperar un frame antes de activar la cámara principal
        yield return new WaitForSeconds(0.3f + 3 * TooltipUI.instance.scheduledTexts.Count);

        TooltipUI.instance.HideTooltipText();

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

        TooltipUI.instance.ShowTooltipText(attacker.name + " uses " + ability.name);

        Vector3 attackerStartPos = attacker.transform.position;
        float t = 0;
        float elapsedTime = 0;

        Unit[] targets = new Unit[1];

        yield return new WaitForSeconds(1f);

        int hits = 1;
        if (ability.multiHit) hits = ability.hits;
        else if (ability.multiHitRange) hits = Random.Range(ability.hitRange[0], ability.hitRange[1]);

        bool nextAttack = false;

        for (int i = 0; i < hits; i++)
        {
            if (!allUnits.Contains(target))
            {
                if (enemyUnits.Count > 0) target = enemyUnits[Random.Range(0, enemyUnits.Count)];
                else break;
            }

            targets[0] = target;

            if (ability.vfxPrefab)
            {
                if (ability.spawnVfxOnSelf)
                {
                    Vector3 dir = target.transform.position - attackerStartPos;
                    GameObject vfx = Instantiate(ability.vfxPrefab, attackerStartPos + .1f * dir, Quaternion.LookRotation(dir));
                    if (ability.sfx) AudioManager.instance.PlaySound3D(ability.sfx, attackerStartPos);
                    yield return new WaitForSeconds(vfx.GetComponent<ParticleSystem>().main.duration / 2);
                    nextAttack = Damage(ability, attacker, targets);
                    yield return new WaitForSeconds(vfx.GetComponent<ParticleSystem>().main.duration / 2);
                    Destroy(vfx);
                }
                else
                {
                    GameObject vfx = Instantiate(ability.vfxPrefab, target.transform);
                    if (ability.sfx) AudioManager.instance.PlaySound3D(ability.sfx, target.transform.position);
                    yield return new WaitForSeconds(vfx.GetComponent<ParticleSystem>().main.duration / 2);
                    nextAttack = Damage(ability, attacker, targets);
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
                if (ability.sfx) AudioManager.instance.PlaySound3D(ability.sfx, attackerEndPos);
                nextAttack = Damage(ability, attacker, targets);

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

                t = 0;
            }
            if(!nextAttack) break;
            yield return new WaitForSeconds(0.1f);
        }

        CameraManager.instance.SetBlendTime(1);
        
        // Importante: Esperar un frame antes de activar la cámara principal
        yield return new WaitForSeconds(0.3f + 3*TooltipUI.instance.scheduledTexts.Count);

        TooltipUI.instance.HideTooltipText();

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

    bool Damage(Abilities ability, Unit attacker, Unit[] targets)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i].evasive)
            {
                TooltipUI.instance.ShowTooltipText(targets[i].name + " evaded the attack!");
                if (ability.condition1 == AbilityCondition.ATTACKMISSED ||
                    ability.condition2 == AbilityCondition.ATTACKMISSED)
                {
                    ResolveAbilityEffect(attacker, targets[i], ability, ability.effect1, ability.effect1Chance, ability.affectSelf, ability.condition1, true);
                    ResolveAbilityEffect(attacker, targets[i], ability, ability.effect2, ability.effect2Chance, ability.affectSelf, ability.condition2, true);
                }
                targets[i].evasive = false;
                if (ability.endOnMiss) return false;
                continue;
            }
            if (!CheckHit(ability, attacker.precision))
            {
                TooltipUI.instance.ShowTooltipText(attacker.name + " missed");
                if(ability.condition1 == AbilityCondition.ATTACKMISSED ||
                    ability.condition2 == AbilityCondition.ATTACKMISSED)
                {
                    ResolveAbilityEffect(attacker, targets[i], ability, ability.effect1, ability.effect1Chance, ability.affectSelf, ability.condition1, true);
                    ResolveAbilityEffect(attacker, targets[i], ability, ability.effect2, ability.effect2Chance, ability.affectSelf, ability.condition2, true);
                }
                if (ability.endOnMiss) return false;
                continue;
            }
            if (ability.power == 0 && targets[i].currentStance == Stance.CAUTIOUS)
            {
                TooltipUI.instance.ShowTooltipText("It doesn't affect " + targets[i].name);
                if (ability.condition1 == AbilityCondition.ATTACKMISSED ||
                    ability.condition2 == AbilityCondition.ATTACKMISSED)
                {
                    ResolveAbilityEffect(attacker, targets[i], ability, ability.effect1, ability.effect1Chance, ability.affectSelf, ability.condition1, true);
                    ResolveAbilityEffect(attacker, targets[i], ability, ability.effect2, ability.effect2Chance, ability.affectSelf, ability.condition2, true);
                }
                if (ability.endOnMiss) return false;
                continue;
            }
            ResolveAbilityEffect(attacker, targets[i], ability, ability.effect1, ability.effect1Chance, ability.affectSelf, ability.condition1);
            ResolveAbilityEffect(attacker, targets[i], ability, ability.effect2, ability.effect2Chance, ability.affectSelf, ability.condition2);

            if (ability.power != 0)
            {
                if(ability.effect1 == AbilityEffect.HEALATTACK || ability.effect2 == AbilityEffect.HEALATTACK)
                {
                    targets[i].Heal(CalculateAttackDamage(attacker, targets[i], ability));
                }
                else
                {
                    targets[i].TakeDamage(CalculateAttackDamage(attacker, targets[i], ability));
                }

                attacker.ResolvePassiveEffect(ExecutionTime.ONHIT, targets[i]);
                attacker.ResolveItemEffect(ExecutionTime.ONHIT, targets[i]);
            }
        }

        if (ability.effect1 == AbilityEffect.INMOLATE) attacker.TakeDamage(int.MaxValue);

        return true;
    }
    void ResolveAbilityEffect(Unit attacker, Unit target, Abilities ability, AbilityEffect effect, float effectChance, bool affectSelf, AbilityCondition condition = AbilityCondition.NONE, bool abilityMissed = false)
    {
        switch (condition)
        {
            case AbilityCondition.NONE:
                break;
            case AbilityCondition.HASSTANCE:
                if (target.currentStance != ability.stanceCondition) return;
                break;
            case AbilityCondition.ISFIRSTROUND:
                if (round > 0) return;
                break;
            case AbilityCondition.HASANYSTATUS:
                if(attacker.status == Status.NONE) return;
                break;
            case AbilityCondition.ATTACKMISSED:
                if(!abilityMissed) return;
                break;
            default:
                break;
        }

        if (attacker.baseEffectChanceMulti*effectChance + attacker.effectChanceModifier >= UnityEngine.Random.Range(1, 101))
        {
            switch (effect)
            {
                case GameData.AbilityEffect.NONE:
                    break;
                case GameData.AbilityEffect.HEAL:
                    if (affectSelf) attacker.HealPercent(ability.healPercentage);
                    else target.HealPercent(ability.healPercentage);
                        break;
                case GameData.AbilityEffect.STATMOD:
                    for (int i = 0; i < ability.statToMod.Length; i++)
                    {
                        if (affectSelf) attacker.ApplyStatModifier(ability.statToMod[i], ability.statMod[i]);
                        else target.ApplyStatModifier(ability.statToMod[i], ability.statMod[i]);
                    }
                    break;
                case GameData.AbilityEffect.STANCECHANGE:
                    if(affectSelf) attacker.ChangeStance(ability.stanceToChangeTo);
                    else target.ChangeStance(ability.stanceToChangeTo);
                    break;
                case GameData.AbilityEffect.APPLYSTATUS:
                    if (affectSelf) attacker.ApplyStatus(ability.status);
                    else target.ApplyStatus(ability.status);
                    break;
                case AbilityEffect.CURESTATUS:
                    target.CureStatus();
                    break;
                case AbilityEffect.FLINCH:
                    target.skipTurn = true;
                    break;
                case AbilityEffect.SETEVASIVE:
                    if (affectSelf) attacker.evasive = true;
                    else target.evasive = true;
                    break;
                case AbilityEffect.PROVOKE:
                    if (affectSelf) attacker.provoking = true;
                    else target.provoking = true;
                    break;
                case AbilityEffect.SETGUARDIAN:
                    attacker.guardedBy = target;
                    break;
                case AbilityEffect.LOSEHP:
                    attacker.TakeDamage(attacker.GetRawStat(Stats.HP, attacker.level)/4);
                    break;
                case AbilityEffect.SWAPSTATS:
                    attacker.SwapStats(ability.statToMod[0], ability.statToMod[1]);
                    break;
                default:
                    break;
            }
        }
    }
    bool CheckHit(Abilities ability, float precision)
    {
        if(ability.effect1 == AbilityEffect.CANTMISS || ability.effect2 == AbilityEffect.CANTMISS) return true;
        if (ability.accuracy * precision >= Random.Range(1,101)) return true;
        else return false;
    }
    int CalculateAttackDamage(Unit attacker, Unit target, Abilities ability)
    {
        int power = ability.power;
        switch (ability.powerVariables)
        {
            case AbilityPowerVariables.REMAININGHP:
                power = (int)(power * 5*(1 - (float)attacker.currentHp/attacker.maxHp));
                break;
            case AbilityPowerVariables.DUPEONALLYDOWNED:
                power = (int)(power * Mathf.Pow(2, PlayerData.teamData.Count - playerUnits.Count));
                break;
            default:
                power = ability.power;
                break;
        }
        float baseCritChance = 0.01f;
        int attackStat = attacker.GetStat(ability.statToCalcDmgWith);
        int defenseStat = target.GetStat(Stats.DEF);
        float stanceBonus = attacker.currentStance == ability.stance ? 1.5f : 1;
        float efficacy = GetAbilityEfficacy(ability.stance, target.currentStance);
        float roll = UnityEngine.Random.Range(.8f, 1f);
        float chanceToCrit = 1f - Mathf.Pow(1 - baseCritChance, attacker.GetStat(Stats.LUCK));
        if(ability.effect1 == AbilityEffect.DOUBLECRITCHANCE || ability.effect2 == AbilityEffect.DOUBLECRITCHANCE) chanceToCrit *= 2;
        bool isCritical = UnityEngine.Random.Range(1, 101) <= chanceToCrit*100;
        float critMod = isCritical ? 1.5f : 1f;
        float freezeMod = target.status == Status.FROZEN ? 1.5f : 1f;

        if (isCritical) 
        {
            if (target.HasPassive("Danger Alarm"))
            {
                TooltipUI.instance.ShowTooltipText($"{target.name}'s danger alarm ability activates");
                TooltipUI.instance.ShowTooltipText($"{target.name} evades the attack");
                ResolveAbilityEffect(attacker, target, ability, ability.effect1, ability.effect1Chance, ability.affectSelf, ability.condition1, true);
                ResolveAbilityEffect(attacker, target, ability, ability.effect2, ability.effect2Chance, ability.affectSelf, ability.condition2, true);
                return 0;
            }
            defenseStat = Mathf.Min(defenseStat, target.GetSetStat(Stats.DEF));
        } 

        float baseDamage = ((2 * attacker.level + 2) * .1f * power * attackStat) / (5.0f * defenseStat);
        float totalBeforeModifiers = baseDamage + 2;
        float finalDamageFloat = totalBeforeModifiers * efficacy * stanceBonus * roll * critMod * freezeMod;
        int damage = Mathf.FloorToInt(finalDamageFloat);

        if (damage <= 0) damage = 1;

        if (ability.effect1 == AbilityEffect.LEECH || ability.effect2 == AbilityEffect.LEECH) attacker.Heal((int)(damage * .5f));
        if (ability.effect1 == AbilityEffect.RECOIL || ability.effect2 == AbilityEffect.RECOIL) attacker.TakeDamage((int)(damage * .5f));

        Debug.Log($"--- REPORTE DE DAÑO ---");
        Debug.Log($"{attacker.name} usó habilidad con Poder: {power} y Stat Ofensivo: {attackStat}");
        Debug.Log($"Defensa de {target.name}: {defenseStat}");
        Debug.Log($"Daño Base calculado (sin modificadores): {baseDamage}");
        Debug.Log($"Modificadores -> Efficacy: {efficacy}, Stance: {stanceBonus}, Roll: {roll}, Crit: {critMod}");
        Debug.Log($"Daño Final antes de redondear: {finalDamageFloat} -> Daño Aplicado: {damage}");

        if (efficacy == 2) TooltipUI.instance.ShowTooltipText("It's super effective!");
        if (isCritical) TooltipUI.instance.ShowTooltipText("Critical Hit!");

        return damage;
    }

    private void CalculateTurnOrder(List<Unit> allUnits)
    {
        allUnits.Sort(delegate (Unit x, Unit y)
        {
            return y.GetStat(Stats.SPEED).CompareTo(x.GetStat(Stats.SPEED));
        });
    }

    public IEnumerator Run<T>(IEnumerator target, Action<T> output)
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