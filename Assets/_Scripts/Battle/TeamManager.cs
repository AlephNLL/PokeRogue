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
    public List<UnitData> startTeamData;

    //temnporal hasta que se elija el equipo en la guarderia
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (PlayerData.teamData == null) PlayerData.teamData = new List<UnitData>();
        if (PlayerData.teamData.Count <= 0)
        {
            PlayerData.teamData = new List<UnitData>();

            for (int i = 0; i < teamData.Count; i++)
            {
                UnitData unit = ScriptableObject.CreateInstance<UnitData>();
                unit.id = i;
                unit.name = teamData[i].name;
                unit.prefab = teamData[i].prefab;
                unit.level = teamData[i].level;
                unit.currentHp = teamData[i].prefab.GetComponent<Unit>().GetRawStat(Stats.HP, unit.level);
                if (teamData[i].knownAbilities.Count > 0) unit.knownAbilities = teamData[i].knownAbilities;
                else unit.knownAbilities = teamData[i].prefab.GetComponent<Unit>().GetUnitKnownAbilities(unit.level);
                unit.heldItem = teamData[i].heldItem;

                PlayerData.teamData.Add(unit);
            }
            teamData = PlayerData.teamData;
        }
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

    public void HealMon(UnitData mon, float healingPercent)
    {
        mon.currentHp = (int)(mon.currentHp + mon.prefab.GetComponent<Unit>().GetRawStat(Stats.HP, mon.level) * healingPercent);
        int maxHp = mon.prefab.GetComponent<Unit>().GetRawStat(Stats.HP, mon.level);
        if (mon.currentHp > maxHp) mon.currentHp = maxHp;
    }
    public void HealTeam(float healingPercent)
    {
        for (int i = 0; i < teamData.Count; i++)
        {
            teamData[i].status = Status.NONE;
            teamData[i].currentHp = (int)(teamData[i].currentHp + teamData[i].prefab.GetComponent<Unit>().GetRawStat(Stats.HP, teamData[i].level) * healingPercent);
            int maxHp = teamData[i].prefab.GetComponent<Unit>().GetRawStat(Stats.HP, teamData[i].level);
            if (teamData[i].currentHp > maxHp) teamData[i].currentHp = maxHp;
        }

        PlayerData.teamData = teamData;
    }

    public void DamageTeam(float damagePercent)
    {
        for (int i = 0; i < teamData.Count; i++)
        {
            teamData[i].currentHp = (int)(teamData[i].currentHp - teamData[i].prefab.GetComponent<Unit>().GetRawStat(Stats.HP, teamData[i].level) * damagePercent);
            if (teamData[i].currentHp <= 0) teamData[i].currentHp = 1;
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
        Unit unit = mon.GetComponent<Unit>();

        UnitData unitData = ScriptableObject.CreateInstance<UnitData>();
        unitData.currentHp = unit.GetRawStat(Stats.HP, BattleData.enemyLevel);
        unitData.id = PlayerData.teamData.Count;
        unitData.level = BattleData.enemyLevel;
        unitData.name = unit.name;
        unitData.prefab = mon;
        unitData.knownAbilities = unit.GetUnitKnownAbilities(unitData.level);

        if (PlayerData.teamData.Count < 4)
        {
            // Si hay espacio, se añade normal
            PlayerData.teamData.Add(unitData);
            EndScreenManager.instance.EndMonSelection(unit);
        }
        else
        {
            // Si está lleno, le pasamos la pelota al EndScreenManager para que pregunte
            EndScreenManager.instance.PromptDaycareSelection(unitData, unit);
        }
    }


}
