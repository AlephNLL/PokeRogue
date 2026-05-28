using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    // Mons y Stats
    public int gold;
    public List<Item> p_items;
    public List<Item> items;
    public List<UnitSaveData> teamData;
    public List<UnitSaveData> daycareTeamData;

    // Mapa
    public List<MapNodeData> mapData;
    public bool mapCreated;
    public string currentRoom;

    // Progresion
    public bool tutorial;
    public bool beatenFirstBoss;

    // Escena
    public string sceneName;

    // Constructor valores iniciales
    public GameSaveData()
    {
        gold = 0;
        p_items = PlayerData.Instance.p_items;
        items = PlayerData.items;
        teamData = GameSaveManager.startTeam;

        daycareTeamData = new List<UnitSaveData>();

        mapData = new List<MapNodeData>();
        mapCreated = false;
        currentRoom = "Spawn-0";
        sceneName = "MapGeneration";
        tutorial = true;
        beatenFirstBoss = false;
    }
}
