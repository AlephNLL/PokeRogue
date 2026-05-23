
using GameData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

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
        AudioManager.instance.PlayMusic(AudioLibrary.instance.daycareMusic);
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
                unit.currentHp = startingUnits[i].prefab.GetComponent<Unit>().GetRawStat(Stats.HP, unit.level);
                if (startingUnits[i].knownAbilities.Count > 0) unit.knownAbilities = startingUnits[i].knownAbilities;
                else unit.knownAbilities = startingUnits[i].prefab.GetComponent<Unit>().GetUnitKnownAbilities(unit.level);
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

    UnitData GenerateNewUnit(UnitData unit1, UnitData unit2)
    {
        UnitData speciesParent = Random.Range(0, 2) == 0 ? unit1 : unit2;
        UnitData abilityParent = speciesParent == unit1 ? unit2 : unit1;

        UnitData unit = ScriptableObject.CreateInstance<UnitData>();
        unit.name = speciesParent.name;
        unit.prefab = speciesParent.prefab;
        unit.level = 1;

        Unit speciesUnitComponent = unit.prefab.GetComponent<Unit>();
        unit.currentHp = speciesUnitComponent.GetRawStat(Stats.HP, 1);

        unit.knownAbilities = new List<Abilities>();

        if (speciesUnitComponent.abilityPool.Length > 0)
            unit.knownAbilities.Add(speciesUnitComponent.abilityPool[0]);
        if (speciesUnitComponent.abilityPool.Length > 1)
            unit.knownAbilities.Add(speciesUnitComponent.abilityPool[1]);

        List<Abilities> inheritableAbilities = new List<Abilities>();
        foreach (Abilities parentAbility in abilityParent.knownAbilities)
        {
            if (!unit.knownAbilities.Contains(parentAbility))
            {
                inheritableAbilities.Add(parentAbility);
            }
        }

        if (inheritableAbilities.Count > 0)
        {
            Abilities inheritedAbility = inheritableAbilities[Random.Range(0, inheritableAbilities.Count)];
            unit.knownAbilities.Add(inheritedAbility);
        }

        units.Remove(unit1);
        units.Remove(unit2);

        unitPrefabs.Remove(unit1.prefab);
        unitPrefabs.Remove(unit2.prefab);

        return unit;
    }

    public void StartBattleSelection()
    {
        TooltipUI.instance.InstantShowText("Select your team");
        DaycareUIManager.instance.DisableAllButtons();
        DaycareCamera.instance.EnableFusionCamera();
        DaycareCamera.instance.SetCameraTarget(unitPrefabs[0].transform);
        StartCoroutine(MonBattleSelection());
    }
    public void StartMonFusionSelection()
    {
        TooltipUI.instance.InstantShowText("Select 2 mons");
        DaycareUIManager.instance.HideMainButtons();
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

            if(selection == -1)
            {
                Cancel();
                yield break;
            }
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

            TooltipUI.instance.InstantShowText($"Select {unitsToSelect - selectedUnits.Count} mons");
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
        DaycareCamera.instance.DisableFusionCamera();
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
        UIManager.Instance.ShowCanvas(false);
        DaycareCamera.instance.DisableFusionCamera();

        yield return new WaitForSeconds(2);

        TooltipUI.instance.InstantShowText("Is this your team?");

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

        if (MapManager.instance)
        {
            MapManager.instance.LoadMapSceneFromStart();
        }
        else
        {
            SceneManager.LoadSceneAsync("MapGeneration");
        }
    }
    IEnumerator FusionConfirmation()
    {
        isBattle = false;

        TooltipUI.instance.HideTooltipText();
        UIManager.Instance.ShowCanvas(false);
        DaycareUIManager.instance.HideMainButtons();

        DaycareCamera.instance.DisableFusionCamera();

        yield return new WaitForSeconds(2);

        TooltipUI.instance.InstantShowText("Do you want to fuse these mons?");

        StartCoroutine(MoveMonsToFront());

        DaycareUIManager.instance.ShowConfirmScreen();
    }
    IEnumerator SelectMon(int monSelection)
    {
        int selection = monSelection;

        bool toggle = true;

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.mouseScrollDelta.sqrMagnitude < 0)
            {
                selection++;

                if (selection >= unitPrefabs.Count)
                {
                    selection = 0;
                }

                DaycareCamera.instance.SetCameraTarget(unitPrefabs[selection].transform);
                UIManager.Instance.UpdateStats(units[selection]);
                if (UIManager.Instance.abilities.activeInHierarchy)
                {
                    UIManager.Instance.UpdateAbilities(units[selection]);
                }
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.mouseScrollDelta.sqrMagnitude > 0)
            {
                selection--;

                if (selection < 0)
                {
                    selection = unitPrefabs.Count - 1;
                }

                DaycareCamera.instance.SetCameraTarget(unitPrefabs[selection].transform);
                UIManager.Instance.UpdateStats(units[selection]);
                if (UIManager.Instance.abilities.activeInHierarchy)
                {
                    UIManager.Instance.UpdateAbilities(units[selection]);
                }
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                yield return selection;
                yield break;
            }
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                UIManager.Instance.ShowCanvas(false);
                yield return -1;
                yield break;
            }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (toggle)
                {
                    UIManager.Instance.ShowCanvas(true, .3f);
                    UIManager.Instance.UpdateStats(units[selection]);
                    UIManager.Instance.UpdateAbilities(units[selection]);
                    StartCoroutine(DaycareCamera.instance.LerpCameraOffset(2, .3f));
                    toggle = false;
                }
                else
                {
                    yield return DaycareCamera.instance.LerpCameraOffset(0, .3f);
                    UIManager.Instance.ShowCanvas(false);
                    toggle = true;
                }
                
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
        TooltipUI.instance.InstantShowText($"Wow! A {newUnit.GetComponent<Unit>().name}");

        selectedPrefabs.Clear();
        selectedUnits.Clear();

        yield return new WaitForSeconds(2f);

        Destroy(newUnit);
        FresnelApplier.applyFresnel(unitPrefabs[^1], Color.blue);
        TooltipUI.instance.HideTooltipText();
        DaycareUIManager.instance.ShowMainButtons();

        startingUnits = units;

        yield return new WaitForSeconds(3f);

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
