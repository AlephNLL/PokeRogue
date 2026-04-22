using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;
    public int gold;
    public List<Item> p_items;
    public static List<Item> items;
    public static List<GameObject> playerTeam;
    public static List<UnitData> teamData;

    private void Awake()
    {
        Instance = this;
        items = p_items;
    }
}
