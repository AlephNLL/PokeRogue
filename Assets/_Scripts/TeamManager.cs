using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public static TeamManager instance;
    public List<UnitData> teamData;
    public List<GameObject> playerUnits;

    private void Start()
    {
        instance = this;
        InitializeTeamData();
    }
    void InitializeTeamData()
    {
        PlayerData.playerTeam = playerUnits;
        PlayerData.teamData = new List<UnitData>();

        for (int i = 0; i < PlayerData.playerTeam.Count; i++)
        {
            Unit unit = PlayerData.playerTeam[i].GetComponent<Unit>();
            UnitData unitData = ScriptableObject.CreateInstance<UnitData>();
            unitData.id = i;
            unitData.currentHp = unit.constitution * unit.level + 1;
            unitData.level = unit.level;
            unitData.name = unit.name;
            PlayerData.teamData.Add(unitData);
        }

        teamData = PlayerData.teamData;
    }
    public void SaveTeamData(List<Unit> playerTeam)
    {
        // Remove dead mons
        List<UnitData> newTeamData = new List<UnitData>();
        List<GameObject> newTeam = new List<GameObject>();

        if (PlayerData.teamData.Count > playerTeam.Count)
        {
            for (int i = 0; i < playerTeam.Count; i++)
            {
                UnitData foundData = PlayerData.teamData.Find(item => item.id == playerTeam[i].id);
                if (foundData) 
                {
                    newTeamData.Add(foundData);
                    newTeam.Add(PlayerData.playerTeam[i]);
                } 
            }

            PlayerData.teamData = newTeamData;
            PlayerData.playerTeam = newTeam;
        }

        //Set variables
        for (int i = 0; i < PlayerData.teamData.Count; i++)
        {
            PlayerData.teamData[i].currentHp = playerTeam[i].currentHp;
            PlayerData.teamData[i].id = playerTeam[i].id;
            PlayerData.teamData[i].level = playerTeam[i].level;
        }

        teamData = PlayerData.teamData;
        playerUnits = PlayerData.playerTeam;
    }
}
