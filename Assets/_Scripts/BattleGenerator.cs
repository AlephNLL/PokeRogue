using GameData;
using UnityEngine;

public class BattleGenerator : MonoBehaviour
{
    public static BattleGenerator Instance;
    public GameObject[] playerTeam;

    [Header("Enemy Pools")]
    public GameObject[] easyEnemyPool;
    public GameObject[] normalEnemyPool;
    public GameObject[] hardEnemyPool;

    GameObject[] enemyPrefabs;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetPlayerTeam();
        }
    }

    void SetPlayerTeam()
    {
        PlayerData.playerTeam = playerTeam;
    }
    public void GenerateTeam(Difficulty difficulty)
    {

        int teamSize = 0;

        switch (difficulty)
        {
            case Difficulty.EASY:
                teamSize = Random.Range(1,4);
                enemyPrefabs = new GameObject[teamSize];
                for (int i = 0; i < enemyPrefabs.Length; i++)
                {
                    enemyPrefabs[i] = easyEnemyPool[Random.Range(0, easyEnemyPool.Length)];
                }
                break;
            case Difficulty.NORMAL:
                teamSize = Random.Range(1, 5);
                enemyPrefabs = new GameObject[teamSize];
                for (int i = 0; i < enemyPrefabs.Length; i++)
                {
                    enemyPrefabs[i] = normalEnemyPool[Random.Range(0, normalEnemyPool.Length)];
                }
                break;
            case Difficulty.HARD:
                teamSize = Random.Range(2, 5);
                enemyPrefabs = new GameObject[teamSize];
                for (int i = 0; i < enemyPrefabs.Length; i++)
                {
                    enemyPrefabs[i] = hardEnemyPool[Random.Range(0, hardEnemyPool.Length)];
                }
                break;
            default:
                return;
        }


        

        BattleData.enemyTeam = enemyPrefabs;
    }
}
