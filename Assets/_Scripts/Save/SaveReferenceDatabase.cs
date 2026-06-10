using System.Collections.Generic;
using UnityEngine;

public class SaveReferenceDatabase : MonoBehaviour
{
    public static SaveReferenceDatabase Instance;

    [Header("Units")]
    [SerializeField] private List<GameObject> unitPrefabs = new();

    [Header("Map Rooms")]
    [SerializeField] private List<GameObject> roomPrefabs = new();

    [Header("Items")]
    [SerializeField] private List<Item> items = new();

    [Header("Abilities")]
    [SerializeField] private List<Abilities> abilities = new();

    private readonly Dictionary<string, GameObject> unitPrefabLookup = new();
    private readonly Dictionary<string, GameObject> roomPrefabLookup = new();
    private readonly Dictionary<string, Item> itemLookup = new();
    private readonly Dictionary<string, Abilities> abilityLookup = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildLookups();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void BuildLookups()
    {
        BuildGameObjectLookup(unitPrefabs, unitPrefabLookup);
        BuildGameObjectLookup(roomPrefabs, roomPrefabLookup);
        BuildObjectLookup(items, itemLookup);
        BuildObjectLookup(abilities, abilityLookup);
    }

    private void BuildGameObjectLookup(List<GameObject> source, Dictionary<string, GameObject> lookup)
    {
        lookup.Clear();
        foreach (GameObject value in source)
        {
            if (value == null || string.IsNullOrEmpty(value.name)) continue;
            lookup[value.name] = value;
        }
    }

    private void BuildObjectLookup<T>(List<T> source, Dictionary<string, T> lookup) where T : UnityEngine.Object
    {
        lookup.Clear();
        foreach (T value in source)
        {
            if (value == null || string.IsNullOrEmpty(value.name)) continue;
            lookup[value.name] = value;
        }
    }

    public static string GetId(UnityEngine.Object value)
    {
        return value == null ? string.Empty : value.name;
    }

    public GameObject GetUnitPrefab(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (unitPrefabLookup.TryGetValue(id, out GameObject prefab)) return prefab;

        if (TeamManager.instance != null)
        {
            foreach (UnitData unitData in TeamManager.instance.teamData)
            {
                if (unitData != null && unitData.prefab != null && unitData.prefab.name == id) return unitData.prefab;
            }
        }

        Debug.LogError($"Missing saved unit prefab reference: {id}");
        return null;
    }

    public GameObject GetRoomPrefab(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (roomPrefabLookup.TryGetValue(id, out GameObject prefab)) return prefab;

        Debug.LogWarning($"Missing saved room prefab reference: {id}");
        return null;
    }

    public Item GetItem(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (itemLookup.TryGetValue(id, out Item item)) return item;

        if (PlayerData.Instance != null && PlayerData.Instance.p_items != null)
        {
            foreach (Item playerItem in PlayerData.Instance.p_items)
            {
                if (playerItem != null && GetId(playerItem) == id) return playerItem;
            }
        }

        if (PlayerData.items != null)
        {
            foreach (Item playerItem in PlayerData.items)
            {
                if (playerItem != null && GetId(playerItem) == id) return playerItem;
            }
        }

        Debug.LogError($"Missing saved item reference: {id}");
        return null;
    }

    public Abilities GetAbility(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (abilityLookup.TryGetValue(id, out Abilities ability)) return ability;

        if (TeamManager.instance != null)
        {
            Abilities teamAbility = FindAbilityInUnitData(TeamManager.instance.teamData, id);
            if (teamAbility != null) return teamAbility;
        }

        Abilities playerAbility = FindAbilityInUnitData(PlayerData.teamData, id);
        if (playerAbility != null) return playerAbility;

        GameObject[] teamPrefabs = PlayerData.Instance != null ? PlayerData.Instance.GetTeamPrefabs() : null;
        if (teamPrefabs != null)
        {
            foreach (GameObject prefab in teamPrefabs)
            {
                if (prefab == null) continue;
                Unit unit = prefab.GetComponent<Unit>();
                if (unit == null || unit.abilityPool == null) continue;

                foreach (Abilities candidate in unit.abilityPool)
                {
                    if (candidate != null && GetId(candidate) == id) return candidate;
                }
            }
        }

        Debug.LogError($"Missing saved ability reference: {id}");
        return null;
    }

    private Abilities FindAbilityInUnitData(List<UnitData> units, string id)
    {
        if (units == null) return null;

        foreach (UnitData unitData in units)
        {
            if (unitData == null) continue;

            if (unitData.knownAbilities != null)
            {
                foreach (Abilities ability in unitData.knownAbilities)
                {
                    if (ability != null && GetId(ability) == id) return ability;
                }
            }

            if (unitData.prefab == null) continue;
            Unit unit = unitData.prefab.GetComponent<Unit>();
            if (unit == null || unit.abilityPool == null) continue;

            foreach (Abilities ability in unit.abilityPool)
            {
                if (ability != null && GetId(ability) == id) return ability;
            }
        }

        return null;
    }
}
