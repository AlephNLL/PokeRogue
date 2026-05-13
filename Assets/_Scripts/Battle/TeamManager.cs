using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;
using Random = UnityEngine.Random;

public class TeamManager : MonoBehaviour
{
    public static TeamManager instance;
    public List<UnitData> teamData;

    //temnporal hasta que se elija el equipo en la guarderia
    private void Awake()
    {
        if (PlayerData.teamData.Count == 0) PlayerData.teamData = teamData;
        else teamData = PlayerData.teamData;
    }
    private void Start()
    {
        instance = this;
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
                } 
            }

            PlayerData.teamData = newTeamData;
        }

        //Set variables
        for (int i = 0; i < PlayerData.teamData.Count; i++)
        {
            PlayerData.teamData[i].currentHp = playerTeam[i].currentHp;
            PlayerData.teamData[i].id = i;
            PlayerData.teamData[i].level = playerTeam[i].level;
            PlayerData.teamData[i].status = playerTeam[i].status;
        }

        teamData = PlayerData.teamData;
    }

    public void HealTeam(float healingPercent)
    {
        for (int i = 0; i < teamData.Count; i++)
        {
            teamData[i].currentHp = (int)(teamData[i].currentHp + teamData[i].currentHp * healingPercent);
            int maxHp = teamData[i].prefab.GetComponent<Unit>().constitution * teamData[i].level + 1;
            if (teamData[i].currentHp > maxHp) teamData[i].currentHp = maxHp;
        }

        PlayerData.teamData = teamData;
    }

    public void DamageTeam(float damagePercent)
    {
        List<UnitData> newTeamData = new List<UnitData>();
        for (int i = 0; i < teamData.Count; i++)
        {
            teamData[i].currentHp = (int)(teamData[i].currentHp - teamData[i].currentHp * damagePercent);
            if (teamData[i].currentHp > 0) newTeamData.Add(teamData[i]);
        }

        teamData = newTeamData;

        for (int i = 0; i < teamData.Count; i++)
        {
            teamData[i].id = i;
        }

        PlayerData.teamData = teamData;
    }

    public void ApplyTeamStatus(Status statusToApply, bool randomStatus = false)
    {
        for (int i = 0; i < teamData.Count; i++)
        {
            if (randomStatus)
            {
                teamData[i].status = (Status)Random.Range(0, Enum.GetValues(typeof(Status)).Length);
            }
            else
            {
                teamData[i].status = statusToApply;
            }
        }

        PlayerData.teamData = teamData;
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
        unitData.prefab = mon;
        unitData.knownAbilities = unit.GetUnitKnownAbilities();

        if (PlayerData.teamData.Count < 4)
        {
            PlayerData.teamData.Add(unitData);
            EndScreenManager.instance.EndMonSelection(unit);
        }
        else
        {
            unitData.id = 0;
            if (PlayerData.daycareTeamData == null) PlayerData.daycareTeamData = new List<UnitData>();
            PlayerData.daycareTeamData.Add(unitData);
            EndScreenManager.instance.EndMonSelection(unit, true);
        }
    }

    
}
