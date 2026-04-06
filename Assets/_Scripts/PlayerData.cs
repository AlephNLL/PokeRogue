using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;
    public int gold;
    public List<GameObject> playerTeam;
    public static List<UnitData> teamData;

    private void Awake()
    {
        Instance = this;
    }
}
