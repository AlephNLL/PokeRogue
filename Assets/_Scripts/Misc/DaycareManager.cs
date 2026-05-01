
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using GameData;

public class DaycareManager : MonoBehaviour
{
    public List<UnitData> startingUnits;
    public static List<UnitData> units;
    public List<GameObject> unitPrefabs;
    public int maxUnits = 20;
    [SerializeField]
    GameObject spawnPoint;
    [SerializeField]
    GameObject cameraCenterPoint;
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

    bool isBattle;
    private void Start()
    {
        if (units == null)
        {
            units = new List<UnitData>();
            unitPrefabs = new List<GameObject>();

            for (int i = 0; i < startingUnits.Count; i++)
            {
                UnitData unit = ScriptableObject.CreateInstance<UnitData>();
                unit.name = startingUnits[i].name;
                unit.prefab = startingUnits[i].prefab;
                unit.level = startingUnits[i].level;
                unit.currentHp = startingUnits[i].prefab.GetComponent<Unit>().constitution + 1;
                unit.knownAbilities = startingUnits[i].knownAbilities;
                unit.heldItem = startingUnits[i].heldItem;

                units.Add(unit);
            }
            startingUnits = units;  
        }
        if (PlayerData.daycareTeamData != null)
        {
            units.AddRange(PlayerData.daycareTeamData);
            PlayerData.daycareTeamData.Clear();
            startingUnits = units;
        }
        SpawnUnits();
        HealAll();
    }
    void HealAll()
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].currentHp = units[i].prefab.GetComponent<Unit>().GetRawStat(Stats.HP, units[i].level);
            units[i].status = Status.NONE;
        }
    }
    void SpawnUnits()
    {
        for (int i = 0; i < units.Count; i++)
        {
            Vector3 offset = i / maxUnitsPerShelf * shelfOffset + Vector3.right * unitSpacing * (i % maxUnitsPerShelf);
            GameObject unitPrefab = Instantiate(units[i].prefab.gameObject, spawnPoint.transform.position + offset, Quaternion.Euler(0, 180, 0));
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
        TooltipUI.instance.ShowTooltipText("Select your team");
        DaycareUIManager.instance.DisableAllButtons();
        DaycareCamera.instance.EnableFusionCamera();
        DaycareCamera.instance.SetCameraTarget(unitPrefabs[0].transform);

        StartCoroutine(MonBattleSelection());
    }
    public void StartMonFusionSelection()
    {
        TooltipUI.instance.ShowTooltipText("Select 2 mons");
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
        DaycareUIManager.instance.HideMainButtons();
        DaycareUIManager.instance.ShowBattleConfirm();
        DaycareUIManager.instance.DisableBattleConfirm();

        yield return WaitForSelection(4);
    }
    IEnumerator WaitForSelection(int unitsToSelect)
    {
        int selection = 0;

        while (selectedUnits.Count < unitsToSelect)
        {
            yield return Run<int>(SelectMon(selection), (output) => selection = output);

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

            TooltipUI.instance.ShowTooltipText($"Select {unitsToSelect - selectedUnits.Count} mons");
            if (selectedUnits.Count > 0) DaycareUIManager.instance.EnableBattleConfirm();
            else DaycareUIManager.instance.DisableBattleConfirm();
        }
    }
    public void EndSelection()
    {
        StopAllCoroutines();

        StartCoroutine(BattleConfirmation());
    }
    public void Confirm()
    {
        if (isBattle)
        {
            StartBattle();
        }
        else
        {
            StartCoroutine(FuseMons());
        }
    }
    public void Cancel()
    {
        DaycareUIManager.instance.HideConfirmScreen();
        DaycareUIManager.instance.HideBattleConfirm();
        TooltipUI.instance.HideTooltipText();
        DaycareUIManager.instance.ShowMainButtons();

        selectedUnits.Clear();
        selectedPrefabs.Clear();

        StopAllCoroutines();

        DeleteUnits();
        SpawnUnits();
    }
    IEnumerator BattleConfirmation()
    {
        isBattle = true;

        TooltipUI.instance.HideTooltipText();

        DaycareCamera.instance.DisableFusionCamera();

        yield return new WaitForSeconds(2);

        TooltipUI.instance.ShowTooltipText("Is this your team?");

        StartCoroutine(MoveMonsToFront());

        DaycareUIManager.instance.ShowConfirmScreen();
    }
    void StartBattle()
    {
        DaycareUIManager.instance.HideBattleConfirm();
        TooltipUI.instance.HideTooltipText();
        PlayerData.teamData.AddRange(selectedUnits);
        for (int i = 0; i < selectedUnits.Count; i++)
        {
            units.Remove(selectedUnits[i]);
            selectedUnits[i].id = i;
        }
        selectedUnits.Clear();
        selectedPrefabs.Clear();
        SceneManager.LoadSceneAsync("MapGeneration");
    }
    IEnumerator FusionConfirmation()
    {
        isBattle = false;

        TooltipUI.instance.HideTooltipText();

        DaycareUIManager.instance.HideMainButtons();

        DaycareCamera.instance.DisableFusionCamera();

        yield return new WaitForSeconds(2);

        TooltipUI.instance.ShowTooltipText("Do you want to fuse these mons?");

        StartCoroutine(MoveMonsToFront());

        DaycareUIManager.instance.ShowConfirmScreen();
    }
    IEnumerator SelectMon(int monSelection)
    {
        int selection = monSelection;
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

    IEnumerator MoveMonsToFront()
    {
        for (int i = 0; i < selectedPrefabs.Count; i++)
        {
            GameObject unit = selectedPrefabs[i];
            Vector3 unitStartPos = unit.transform.position;
            float t = 0;
            float elapsedTime = 0;
            Vector3 offset = new Vector3(2 * (i - (selectedPrefabs.Count - 1) / 2f), 0, 0);
            while (t < 1)
            {
                elapsedTime += Time.deltaTime;
                t += elapsedTime * elapsedTime / 10;
                unit.transform.position = Vector3.Lerp(unitStartPos, cameraCenterPoint.transform.position + offset, t);
                yield return null;
            }

            unit.transform.position = cameraCenterPoint.transform.position + offset;
        }
        
    }

    IEnumerator FuseMons()
    {
        DaycareUIManager.instance.HideConfirmScreen();
        TooltipUI.instance.HideTooltipText();

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
            leftUnit.transform.position = Vector3.Lerp(unitLeftStartPos, cameraCenterPoint.transform.position - 3 * Vector3.right, t);
            rightUnit.transform.position = Vector3.Lerp(unitRightStartPos, cameraCenterPoint.transform.position + 3 * Vector3.right, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        t = 0;

        while (t < .8f)
        {
            elapsedTime += Time.deltaTime;
            t += elapsedTime * elapsedTime / 10;
            leftUnit.transform.position = Vector3.Lerp(unitLeftStartPos, cameraCenterPoint.transform.position, t);
            rightUnit.transform.position = Vector3.Lerp(unitRightStartPos, cameraCenterPoint.transform.position, t);
            yield return null;
        }

        leftUnit.transform.position = cameraCenterPoint.transform.position;
        rightUnit.transform.position = cameraCenterPoint.transform.position;

        units.Add(GenerateNewUnit(selectedUnits[0], selectedUnits[1]));

        DeleteUnits();
        SpawnUnits();

        GameObject newUnit = Instantiate(unitPrefabs[^1], cameraCenterPoint.transform.position, Quaternion.Euler(0, 180, 0));
        newUnit.GetComponent<Unit>().enabled = false;
        TooltipUI.instance.ShowTooltipText($"Wow! A {newUnit.GetComponent<Unit>().name}");
        FresnelApplier.applyFresnel(unitPrefabs[^1], Color.white);

        selectedPrefabs.Clear();
        selectedUnits.Clear();

        yield return new WaitForSeconds(2f);

        Destroy(newUnit);
        TooltipUI.instance.HideTooltipText();
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
