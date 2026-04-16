using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

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
            PlayerData.teamData[i].id = i;
            PlayerData.teamData[i].level = playerTeam[i].level;
        }

        teamData = PlayerData.teamData;
        playerUnits = PlayerData.playerTeam;
    }

    public void HealTeam(float healingPercent)
    {
        for (int i = 0; i < teamData.Count; i++)
        {
            teamData[i].currentHp = (int)(teamData[i].currentHp + teamData[i].currentHp * healingPercent);
            int maxHp = playerUnits[i].GetComponent<Unit>().constitution * teamData[i].level + 1;
            if (teamData[i].currentHp > maxHp) teamData[i].currentHp = maxHp;
        }
    }

    public void AddNewTeamMember(GameObject mon)
    {
        EndScreenManager.monSelected = true;
        Unit unit = mon.GetComponent<Unit>();
        UnitData unitData = ScriptableObject.CreateInstance<UnitData>();
        unitData.currentHp = unit.constitution * unit.level + 1;
        unitData.id = PlayerData.teamData.Count;
        unitData.level = unit.level;
        unitData.name = unit.name;

        if (PlayerData.teamData.Count < 4)
        {
            PlayerData.teamData.Add(unitData);
            PlayerData.playerTeam.Add(mon);
            EndScreenManager.instance.EndMonSelection(unit);
        }
        else
        {
            Debug.Log("Team already full, sending to daycare...");
            EndScreenManager.instance.EndMonSelection(unit, true);
        }
    }

    
}
