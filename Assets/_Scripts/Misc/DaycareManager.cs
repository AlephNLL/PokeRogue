
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Random = UnityEngine.Random;

public class DaycareManager : MonoBehaviour
{
    public List<UnitData> startingUnits;
    public static List<UnitData> units = new List<UnitData>();
    public List<GameObject> unitPrefabs;
    public int maxUnits = 20;
    [SerializeField]
    GameObject spawnPoint;
    [SerializeField]
    GameObject[] fusionPoints;
    [SerializeField]
    int maxUnitsPerShelf = 5;
    [SerializeField]
    float unitSpacing = 2;
    [SerializeField]
    Vector3 shelfOffset;
    [SerializeField]
    List<UnitData> selectedUnits = new List<UnitData>();
    [SerializeField]
    List<GameObject> selectedPrefabs = new List<GameObject>();
    private void Start()
    {
        for (int i = 0; i < startingUnits.Count; i++)
        {
            UnitData unit = ScriptableObject.CreateInstance<UnitData>();
            unit.name = startingUnits[i].name;
            unit.prefab = startingUnits[i].prefab;
            unit.level = startingUnits[i].level;
            unit.currentHp = startingUnits[i].prefab.GetComponent<Unit>().constitution + 1;
            unit.knownAbilities = startingUnits[i].knownAbilities;

            units.Add(unit);
        }
        startingUnits = units;
        //units = PlayerData.daycareTeamData;
        //startingUnits = units;
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

            units[i].id = i;
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

        unitPrefabs.Remove(unit1.prefab);
        unitPrefabs.Remove(unit2.prefab);

        return unit;
    }

    public void StartBattleSelection()
    {
        DaycareUIManager.instance.ShowTooltipText("Select your team");
        DaycareUIManager.instance.DisableAllButtons();
        DaycareCamera.instance.EnableFusionCamera();
        DaycareCamera.instance.SetCameraTarget(unitPrefabs[0].transform);

        StartCoroutine(MonBattleSelection());
    }
    public void StartMonFusionSelection()
    {
        DaycareUIManager.instance.ShowTooltipText("Select 2 mons");
        DaycareUIManager.instance.DisableAllButtons();
        DaycareCamera.instance.EnableFusionCamera();
        DaycareCamera.instance.SetCameraTarget(unitPrefabs[0].transform);

        StartCoroutine(MonFusionSelection());
    }
    IEnumerator MonFusionSelection()
    {
        yield return WaitForSelection(2);
        yield return FusionConfirmation();
    }
    IEnumerator MonBattleSelection()
    {
        yield return WaitForSelection(4);
    }
    IEnumerator WaitForSelection(int unitsToSelect)
    {
        int selection = 0;

        while (selectedUnits.Count < unitsToSelect)
        {
            yield return Run<int>(SelectMon(), (output) => selection = output);

            if (!selectedUnits.Any(u => u.id == units[selection].id))
            {
                FresnelApplier.applyFresnel(unitPrefabs[selection], Color.white);
                selectedUnits.Add(units[selection]);
                selectedPrefabs.Add(unitPrefabs[selection]);
            }
            else
            {
                FresnelApplier.clearFresnel(unitPrefabs[selection]);
                selectedUnits.Remove(units[selection]);
                selectedPrefabs.Remove(unitPrefabs[selection]);
            }

            DaycareUIManager.instance.ShowTooltipText($"Select {unitsToSelect - selectedUnits.Count} mons");
        }
    }
    IEnumerator FusionConfirmation()
    {
        DaycareUIManager.instance.HideTooltipText();

        DaycareCamera.instance.DisableFusionCamera();

        yield return new WaitForSeconds(2);

        DaycareUIManager.instance.ShowTooltipText("Do you want to fuse these mons?");

        StartCoroutine(MoveMonsToFusionPoint());

        DaycareUIManager.instance.ShowConfirmScreen();
    }
    IEnumerator SelectMon()
    {
        int selection = 0;
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                selection++;

                if (selection >= unitPrefabs.Count)
                {
                    selection = 0;
                }

                DaycareCamera.instance.SetCameraTarget(unitPrefabs[selection].transform);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                selection--;

                if (selection < 0)
                {
                    selection = unitPrefabs.Count - 1;
                }

                DaycareCamera.instance.SetCameraTarget(unitPrefabs[selection].transform);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                yield return selection;
                yield break;
            }

            yield return null;
        }
    }

    IEnumerator MoveMonsToFusionPoint()
    {
        for (int i = 0; i < selectedPrefabs.Count; i++)
        {
            GameObject unit = selectedPrefabs[i];
            Vector3 unitStartPos = unit.transform.position;
            float t = 0;
            float elapsedTime = 0;

            while (t < 1)
            {
                elapsedTime += Time.deltaTime;
                t += elapsedTime * elapsedTime / 10;
                unit.transform.position = Vector3.Lerp(unitStartPos, fusionPoints[i].transform.position, t);
                yield return null;
            }

            unit.transform.position = fusionPoints[i].transform.position;
        }
        
    }

    public void StartFusion()
    {
        StartCoroutine(FuseMons());
    }

    IEnumerator FuseMons()
    {
        DaycareUIManager.instance.HideConfirmScreen();
        DaycareUIManager.instance.HideTooltipText();

        GameObject leftUnit = selectedPrefabs[0];
        GameObject rightUnit = selectedPrefabs[1];
        Vector3 unitLeftStartPos = leftUnit.transform.position;
        Vector3 unitRightStartPos = rightUnit.transform.position;
        float t = 0;
        float elapsedTime = 0;

        while (t < 1)
        {
            elapsedTime += Time.deltaTime;
            t += elapsedTime * elapsedTime / 10;
            leftUnit.transform.position = Vector3.Lerp(unitLeftStartPos, fusionPoints[2].transform.position - 3 * Vector3.right, t);
            rightUnit.transform.position = Vector3.Lerp(unitRightStartPos, fusionPoints[2].transform.position + 3 * Vector3.right, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        t = 0;

        while (t < .8f)
        {
            elapsedTime += Time.deltaTime;
            t += elapsedTime * elapsedTime / 10;
            leftUnit.transform.position = Vector3.Lerp(unitLeftStartPos, fusionPoints[2].transform.position, t);
            rightUnit.transform.position = Vector3.Lerp(unitRightStartPos, fusionPoints[2].transform.position, t);
            yield return null;
        }

        leftUnit.transform.position = fusionPoints[2].transform.position;
        rightUnit.transform.position = fusionPoints[2].transform.position;

        units.Add(GenerateNewUnit(selectedUnits[0], selectedUnits[1]));

        DeleteUnits();
        SpawnUnits();

        GameObject newUnit = Instantiate(unitPrefabs[^1], fusionPoints[2].transform.position, Quaternion.identity);
        newUnit.GetComponent<Unit>().enabled = false;
        DaycareUIManager.instance.ShowTooltipText($"Wow! A {newUnit.GetComponent<Unit>().name}");
        FresnelApplier.applyFresnel(unitPrefabs[^1], Color.white);

        selectedPrefabs.Clear();
        selectedUnits.Clear();

        yield return new WaitForSeconds(2f);

        Destroy(newUnit);
        DaycareUIManager.instance.HideTooltipText();
        DaycareUIManager.instance.ShowMainButtons();

        startingUnits = units;

        yield return new WaitForSeconds(2f);

        FresnelApplier.clearFresnel(unitPrefabs[^1]);
    }
    public IEnumerator Run<T>(IEnumerator target, Action<T> output)
    {
        object result = null;
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
        output((T)result);
    }
}
