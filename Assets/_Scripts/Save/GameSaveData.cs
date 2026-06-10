using System.Collections.Generic;
using GameData;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    // Mons y Stats
    public int gold;
    public List<string> p_itemIds;
    public List<string> itemIds;
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

    // Opciones
    public int difficulty;

    // Constructor valores iniciales
    public GameSaveData()
    {
        gold = 0;
        p_itemIds = new List<string>();
        itemIds = new List<string>();
        if (PlayerData.Instance != null && PlayerData.Instance.p_items != null)
        {
            foreach (Item item in PlayerData.Instance.p_items)
            {
                p_itemIds.Add(SaveReferenceDatabase.GetId(item));
            }
        }

        if (PlayerData.items != null)
        {
            foreach (Item item in PlayerData.items)
            {
                itemIds.Add(SaveReferenceDatabase.GetId(item));
            }
        }

        teamData = GameSaveManager.startTeam;

        daycareTeamData = new List<UnitSaveData>();

        mapData = new List<MapNodeData>();
        mapCreated = false;
        currentRoom = "Spawn-0";
        sceneName = "MapGeneration";
        tutorial = true;
        beatenFirstBoss = false;

        difficulty = (int)Difficulty.EASY;
    }
}
