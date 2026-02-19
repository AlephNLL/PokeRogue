using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


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
        yield return StartCoroutine(AttackSequence(currentUnit, playerUnits[UnityEngine.Random.Range(0, playerUnits.Count)]));
    }

    public void Attack(Unit attacker) //Se llama desde la interfaz del jugador, los botones se suscriben al activarse
    {
        attacker.SelectTarget(enemyUnits[0].gameObject);
        attacker.DeactivateCamera();
        attacker.CloseBattleMenu();

        StartCoroutine(SelectTarget(attacker));
    }

    IEnumerator SelectTarget(Unit attacker)
    {
        int selection = 0;

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
                StartCoroutine(AttackSequence(attacker, enemyUnits[selection]));
                break;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StartCoroutine(PlayerTurn(attacker));
                attacker.EndSelect();
                break;
            }

            yield return null;
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
    IEnumerator AttackSequence(Unit attacker, Unit target)
    {
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
            t += elapsedTime * elapsedTime/10;
            attacker.transform.position = Vector3.Lerp(attackerStartPos, target.transform.position, t);
            yield return null;
        }

        Debug.Log(attacker.name + " attacks " + target.name + " dealing: " + CalculateAttackDamage(attacker, target) + " damage.");
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

        // Asegurar que regresó a su posición exacta
        attacker.transform.position = attackerStartPos;

        // Importante: Esperar un frame antes de activar la cámara principal
        yield return null;

        CameraManager.instance.ActivateMainCamera();

        currentTurnIndex++;
        // Avanzar al siguiente turno
        StartNextTurn();
    }

    int CalculateAttackDamage(Unit attacker, Unit target)
    {
        int attackStat = attacker.GetAttackStat();
        int defenseStat = target.GetDefenseStat();

        float roll = Random.Range(.8f, 1f);
        bool isCritical = Random.Range(0, 16) == 0;

        return Mathf.FloorToInt((((2 * attacker.level / 5) + 2) * (attackStat / defenseStat) / 50 + 2) * roll);
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
}