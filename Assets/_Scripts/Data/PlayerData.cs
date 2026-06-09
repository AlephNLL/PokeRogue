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

    public static bool tutorial = false;
    public bool beatenFirstBoss = false;

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

    public int GetAverageTeamLevel()
    {
        int sum = 0;

        if (teamData == null) return 0;
        if(teamData.Count <= 0) return 0;

        for (int i = 0; i < teamData.Count; i++)
        {
            sum += teamData[i].level;
        }

        return Mathf.FloorToInt((float)sum/teamData.Count);
    }
    public void SaveData(ref GameSaveData data)
    {
        data.gold = gold;
        data.p_itemIds = SaveItemIds(this.p_items);
        data.itemIds = SaveItemIds(PlayerData.items);
        data.teamData = new();
        data.daycareTeamData = new();
        data.tutorial = tutorial;
        data.beatenFirstBoss = beatenFirstBoss;

        foreach (UnitData unitData in teamData)
        {
            data.teamData.Add(unitData.LoadData());
        }


        if (daycareTeamData == null) return;
        foreach (UnitData unitData in daycareTeamData)
        {
            data.daycareTeamData.Add(unitData.LoadData());
        }
    }

    public void LoadData(GameSaveData data)
    {
        this.gold = data.gold;
        this.p_items = LoadItems(data.p_itemIds);
        PlayerData.items = LoadItems(data.itemIds);
        teamData = new();
        daycareTeamData = new();
        tutorial = data.tutorial;
        beatenFirstBoss = data.beatenFirstBoss;

        foreach (UnitSaveData unitData in data.teamData)
        {
            UnitData empty = (UnitData)ScriptableObject.CreateInstance(typeof(UnitData));
            teamData.Add(empty.SaveData(unitData));
        }

        TeamManager.instance.teamData = teamData;

        if (daycareTeamData == null) return;
        foreach (UnitSaveData unitData in data.daycareTeamData)
        {
            UnitData empty = (UnitData)ScriptableObject.CreateInstance(typeof(UnitData));
            daycareTeamData.Add(empty.SaveData(unitData));
        }
    }

    private List<string> SaveItemIds(List<Item> itemList)
    {
        List<string> itemIds = new();
        if (itemList == null) return itemIds;

        foreach (Item item in itemList)
        {
            itemIds.Add(SaveReferenceDatabase.GetId(item));
        }

        return itemIds;
    }

    private List<Item> LoadItems(List<string> itemIds)
    {
        List<Item> itemList = new();
        if (itemIds == null || SaveReferenceDatabase.Instance == null) return itemList;

        foreach (string itemId in itemIds)
        {
            Item item = SaveReferenceDatabase.Instance.GetItem(itemId);
            if (item != null) itemList.Add(item);
        }

        return itemList;
    }
}
