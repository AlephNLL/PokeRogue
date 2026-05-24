using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    // Mons y Stats
    public int gold;
    public List<string> p_items;
    public List<string> items;
    public List<UnitSaveData> teamData;
    public List<UnitSaveData> daycareTeamData;

    // Mapa
    public List<MapNode> mapData;

    // Progresion
    public bool tutorial = true;
    public bool beatenFirstBoss = true;

    // Constructor valores iniciales
    public GameSaveData()
    {
        gold = 0;
        p_items = new List<string>();
        items = new List<string>();
        teamData = GameSaveManager.startTeam;

        daycareTeamData = new List<UnitSaveData>();

        mapData = new List<MapNode>();
    }
}
