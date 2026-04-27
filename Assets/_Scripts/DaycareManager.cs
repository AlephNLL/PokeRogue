
using System.Collections.Generic;
using UnityEngine;

public class DaycareManager : MonoBehaviour
{
    public List<UnitData> units;
    [SerializeField]
    GameObject spawnPoint;
    [SerializeField]
    int maxUnitsPerShelf = 5;
    [SerializeField]
    Vector3 shelfOffset;

    private void Start()
    {
        SpawnUnits();
    }

    void SpawnUnits()
    {
        for (int i = 0; i < units.Count; i++)
        {
            Vector3 offset = i / maxUnitsPerShelf * shelfOffset + Vector3.right * 2 * (i % maxUnitsPerShelf);
            Instantiate(units[i].prefab.gameObject, spawnPoint.transform.position + offset, Quaternion.identity);
        }
    }
    void DeleteUnits()
    {
        Unit[] unitToDestroy = FindObjectsOfType<Unit>();

        foreach (Unit unit in unitToDestroy) 
        {
            Destroy(unit.gameObject);
        }
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
        //Select a unit to be progenitor
        UnitData parent = Random.Range(0, 2) == 0 ? unit1 : unit2;
        UnitData unit = new UnitData();

        //Choose a random ability to inherit - CAMBIAR PARA QUE NO SEA SOLO HABIULIDADES DEL PROGENITOR
        Abilities childAbility = parent.knownAbilities[Random.Range(0, parent.knownAbilities.Length)];

        unit.name = parent.name;
        unit.prefab = parent.prefab;
        unit.level = 1;
        unit.knownAbilities = new Abilities[1];
        unit.knownAbilities[0] = childAbility;

        return unit;
    }
}
