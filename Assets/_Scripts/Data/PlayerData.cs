using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour, ISaveData
{
    public static PlayerData Instance;
    public int gold;
    public List<Item> p_items;
    public static List<Item> items;
    public static List<UnitData> teamData;
    public static List<UnitData> daycareTeamData;

    public static bool tutorial = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        items = p_items;
        if (teamData == null) { teamData = new List<UnitData>(); }
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

    public void SaveData(ref GameSaveData data)
    {
        data.gold = gold;
        //data.p_items = this.p_items;
        //data.items = PlayerData.items;
        data.teamData = new();
        data.tutorial = tutorial;

        foreach (UnitData unitData in teamData)
        {
            data.teamData.Add(unitData.LoadData());
        }


        if (daycareTeamData == null) return;
        for (int i = 0; i < daycareTeamData.Count; i++)
        {
            data.daycareTeamData[i] = daycareTeamData[i].LoadData();
        }
    }

    public void LoadData(GameSaveData data)
    {
        this.gold = data.gold;
        //this.p_items = data.p_items;
        //PlayerData.items = data.items;
        teamData = new();
        daycareTeamData = new();
        tutorial = data.tutorial;

        foreach (UnitSaveData unitData in data.teamData)
        {
            UnitData empty = (UnitData)ScriptableObject.CreateInstance(typeof(UnitData));
            teamData.Add(empty.SaveData(unitData));
        }

        TeamManager.instance.teamData = teamData;

        if (daycareTeamData == null) return;
        for (int i = 0; i < data.daycareTeamData.Count; i++)
        {
            daycareTeamData[i].SaveData(data.daycareTeamData[i]);
        }
    }
}
