using GameData;
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Necesario para filtrar listas fácilmente

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

    private void Awake()
    {
        Instance = this;
    }

    public void GenerateTeam(Difficulty difficulty)
    {
        int floorLevel = MapManager.instance.currentNode.floorLevel;

        int baseLevel = ((int)difficulty) + 1 + (floorLevel / 2);
        int levelVariance = 0;// Random.Range(0, 2);
        int finalEnemyLevel = Mathf.Max(1, baseLevel + levelVariance);

        int minSize = floorLevel == 0 ? 1 : 2;
        int maxSize = 4;
        int budget = 0;

        switch (difficulty)
        {
            case Difficulty.EASY:
                minSize = 2; maxSize = 3;
                budget = 150 + (floorLevel * 10);
                break;
            case Difficulty.NORMAL:
                minSize = 2; maxSize = 4;
                budget = 250 + (floorLevel * 15);
                break;
            case Difficulty.HARD:
                minSize = 2; maxSize = 5;
                budget = 350 + (floorLevel * 20);
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

        if (generatedTeam.Count == 0)
        {
            GameObject weakestEnemy = enemyPool.OrderBy(e => GetEnemyCost(e)).First();
            generatedTeam.Add(weakestEnemy);
            totalBstInTeam += GetEnemyCost(weakestEnemy);
        }

        BattleData.enemyTeam = generatedTeam.ToArray();
        BattleData.enemyLevel = finalEnemyLevel;

        float rewardMultiplier = ((int)difficulty + 1) * 0.5f; // Easy = 0.5x, Normal = 1x, Hard = 1.5x

        BattleData.goldReward = Mathf.RoundToInt((totalBstInTeam * 0.1f) * rewardMultiplier) + (floorLevel * 5);
        BattleData.expReward = Mathf.RoundToInt((totalBstInTeam * 0.5f) * finalEnemyLevel * rewardMultiplier);
    }

    private int GetEnemyCost(GameObject enemyPrefab)
    {
        Unit unitStats = enemyPrefab.GetComponent<Unit>();
        if (unitStats != null)
        {
            
            int total = unitStats.strength + unitStats.constitution + unitStats.dexterity + unitStats.luck;
            if (cheapEnemies.Contains(enemyPrefab)) total += cheapEnemyExtraCost;
            else if (normalEnemies.Contains(enemyPrefab)) total += normalEnemyExtraCost;
            else if (expensiveEnemies.Contains(enemyPrefab)) total += expensiveEnemyExtraCost;
            return total;
        }

        Debug.LogWarning($"El prefab {enemyPrefab.name} no tiene el componente Unit. Retornando BST por defecto.");
        return 50;
    }
}