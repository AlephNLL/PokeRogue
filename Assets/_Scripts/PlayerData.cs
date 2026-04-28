using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;
    public int gold;
    public List<Item> p_items;
    public static List<Item> items;
    public static List<UnitData> teamData;

    private void Awake()
    {
        Instance = this;
        items = p_items;
    }

    public GameObject[] GetTeamPrefabs()
    {
        List<GameObject> list = new List<GameObject>();

        for (int i = 0; i < teamData.Count; i++)
        {
            list.Add(teamData[i].prefab);
        }

        return list.ToArray();
    }
}
