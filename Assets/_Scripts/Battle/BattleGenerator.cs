using GameData;
using System.Collections.Generic;
using System.Linq; // Necesario para filtrar listas fácilmente
using Unity.VisualScripting;
using UnityEngine;

public class BattleGenerator : MonoBehaviour
{
    public static BattleGenerator Instance;

    [Header("Enemy Pools")]
    public GameObject[] enemyPool;

    public List<GameObject> cheapEnemies;
    public int cheapEnemyExtraCost = 0;
    public List<GameObject> normalEnemies;
    public int normalEnemyExtraCost = 0;
    public List<GameObject> expensiveEnemies;
    public int expensiveEnemyExtraCost = 0;

    public Team[] bossTeams;

    public GameObject[] tutorialTeam;
    private void Awake()
    {
        Instance = this;
    }

    public void GenerateTeam(Difficulty difficulty, bool isBoss = false)
    {
        int floorLevel = MapManager.instance.currentNode.floorLevel;

        int baseLevel = PlayerData.Instance.GetAverageTeamLevel();
        int levelVariance = 0;// Random.Range(0, 2);
        int finalEnemyLevel = Mathf.Max(1, baseLevel + levelVariance);

        int minSize = floorLevel == 0 ? 1 : 2;
        int maxSize = 4;
        int budget = 0;

        switch (difficulty)
        {
            case Difficulty.EASY:
                minSize = 1; maxSize = 4;
                budget = 1500 + (floorLevel * 10);
                break;
            case Difficulty.NORMAL:
                minSize = 2; maxSize = 4;
                budget = 1700 + (floorLevel * 15);
                break;
            case Difficulty.HARD:
                minSize = 2; maxSize = 5;
                budget = 2000 + (floorLevel * 20);
                break;
            default:
                return;
        }

        List<GameObject> generatedTeam = new List<GameObject>();
        int totalBstInTeam = 0;

        while (budget > 0 && generatedTeam.Count < maxSize)
        {
            List<GameObject> affordableEnemies = enemyPool.Where(e => GetEnemyCost(e) <= budget).ToList();

            if (affordableEnemies.Count == 0 || (generatedTeam.Count >= minSize && Random.value < 0.2f))
            {
                break;
            }

            GameObject chosenEnemy = affordableEnemies[Random.Range(0, affordableEnemies.Count)];
            int enemyBst = GetEnemyCost(chosenEnemy);

            generatedTeam.Add(chosenEnemy);
            budget -= enemyBst;
            totalBstInTeam += enemyBst;
        }

        if (generatedTeam.Count < minSize)
        {
            GameObject weakestEnemy = enemyPool.OrderBy(e => GetEnemyCost(e)).First();
            generatedTeam.Add(weakestEnemy);
            totalBstInTeam += GetEnemyCost(weakestEnemy);
        }

        if (isBoss)
        {
            totalBstInTeam = 0;
            generatedTeam.Clear();
            generatedTeam.AddRange(bossTeams[Random.Range(0, bossTeams.Length)].team);
            for (global::System.Int32 i = 0; i < generatedTeam.Count; i++)
            {
                totalBstInTeam += GetEnemyCost(generatedTeam[i]);
            }
            BattleData.isBoss = true;
        }
        else BattleData.isBoss = false;

        if (PlayerData.tutorial)
        {
            totalBstInTeam = 0;
            generatedTeam.Clear();
            generatedTeam.AddRange(tutorialTeam);
            for (global::System.Int32 i = 0; i < generatedTeam.Count; i++)
            {
                totalBstInTeam += GetEnemyCost(generatedTeam[i]);
            }
        }

        BattleData.goldReward = Mathf.RoundToInt((totalBstInTeam * 0.01f));
        BattleData.expReward = Mathf.RoundToInt((totalBstInTeam * 0.02f) * finalEnemyLevel);

        BattleData.enemyTeam = generatedTeam.ToArray();
        BattleData.enemyLevel = finalEnemyLevel;
    }

    private int GetEnemyCost(GameObject enemyPrefab)
    {
        Unit unitStats = enemyPrefab.GetComponent<Unit>();
        if (unitStats != null)
        {
            
            int total = unitStats.strength + unitStats.constitution + unitStats.dexterity;
            if (cheapEnemies.Contains(enemyPrefab)) total += cheapEnemyExtraCost;
            else if (normalEnemies.Contains(enemyPrefab)) total += normalEnemyExtraCost;
            else if (expensiveEnemies.Contains(enemyPrefab)) total += expensiveEnemyExtraCost;
            return total;
        }

        Debug.LogWarning($"El prefab {enemyPrefab.name} no tiene el componente Unit. Retornando BST por defecto.");
        return 50;
    }
}

[System.Serializable]
public class Team
{
    public GameObject[] team;
}