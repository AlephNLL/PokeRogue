
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DaycareManager : MonoBehaviour
{
    public List<UnitData> startingUnits;
    public static List<UnitData> units;
    List<GameObject> unitPrefabs;
    [SerializeField]
    GameObject spawnPoint;
    [SerializeField]
    int maxUnitsPerShelf = 5;
    [SerializeField]
    float unitSpacing = 2;
    [SerializeField]
    Vector3 shelfOffset;

    private void Start()
    {
        units = startingUnits;
        unitPrefabs = new List<GameObject>();
        SpawnUnits();
    }

    void SpawnUnits()
    {
        for (int i = 0; i < units.Count; i++)
        {
            Vector3 offset = i / maxUnitsPerShelf * shelfOffset + Vector3.right * unitSpacing * (i % maxUnitsPerShelf);
            GameObject unitPrefab = Instantiate(units[i].prefab.gameObject, spawnPoint.transform.position + offset, Quaternion.identity);
            unitPrefab.GetComponent<Unit>().enabled = false;
            unitPrefabs.Add(unitPrefab);
        }
    }
    void DeleteUnits()
    {
        Unit[] unitToDestroy = FindObjectsOfType<Unit>();

        foreach (Unit unit in unitToDestroy) 
        {
            Destroy(unit.gameObject);
        }

        unitPrefabs.Clear();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            units.Add(GenerateNewUnit(units[0], units[1]));
            DeleteUnits();
            SpawnUnits();
        }
    }

    UnitData GenerateNewUnit(UnitData unit1, UnitData unit2)
    {
        //Select a unit to inherit species and ability
        UnitData speciesParent = Random.Range(0, 2) == 0 ? unit1 : unit2;
        UnitData abilityParent = Random.Range(0, 2) == 0 ? unit1 : unit2;

        //Choose a random ability to inherit
        Abilities childAbility = abilityParent.knownAbilities[Random.Range(0, abilityParent.knownAbilities.Length)];

        UnitData unit = ScriptableObject.CreateInstance<UnitData>();
        unit.name = speciesParent.name;
        unit.prefab = speciesParent.prefab;
        unit.level = 1;
        unit.currentHp = speciesParent.prefab.GetComponent<Unit>().constitution + 1;
        unit.knownAbilities = new Abilities[1];
        unit.knownAbilities[0] = childAbility;

        units.Remove(unit1);
        units.Remove(unit2);

        return unit;
    }
}
