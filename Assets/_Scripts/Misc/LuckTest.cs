using UnityEngine;

public class LuckTest : MonoBehaviour
{
    public float baseChance;
    public float modifier;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float finalChance;
        string result;
        for (int i = 0; i < 10; i++)
        {
            finalChance = 1 - Mathf.Pow(1 - baseChance, i) * modifier;
            result = $"With {i} luck: {Mathf.FloorToInt(finalChance * 100)}";
            Debug.Log(result);
        }
        for (int i = 0; i < 10; i++)
        {
            finalChance = 1 - Mathf.Pow(1 - baseChance, i*10)*modifier;
            result = $"With {i*10} luck: {Mathf.FloorToInt(finalChance * 100)}";
            Debug.Log(result);
        }
    }
}
