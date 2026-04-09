using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using GameData;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Drawing;
using Unity.VisualScripting;
public class Unit : MonoBehaviour
{
    public int id;
    new public string name;
    public string description;
    public Stance currentStance;
    public int level;
    public Status status;
    private float stanceModifier = 1.5f;


    public int strength;
    public int constitution;
    public int dexterity;
    public int luck;


    [SerializeField]
    public int maxHp;
    [SerializeField]
    public int currentHp;
    [SerializeField]
    private int attack;
    [SerializeField]
    private int defense;
    [SerializeField]
    private int speed;


    public Abilities[] knownAbilities;
    public Abilities[] abilityPool;

    [Header("Misc")]
    [SerializeField]
    private CinemachineVirtualCamera actionCamera;
    [SerializeField]
    private CinemachineVirtualCamera selectionCamera;
    [SerializeField]
    private Canvas battleMenu;
    [SerializeField]
    private Canvas abilityMenu;
    [SerializeField]
    private Canvas statusMenu;
    [SerializeField]
    private Button attackButton;
    [SerializeField]
    private Button runButton;
    [SerializeField]
    private Button[] abilityButtons;
    [SerializeField]
    private Slider healthBar;
    [SerializeField]
    private GameObject nameText;

    public bool isPlayerControlled = false;

    public bool additionalTurn = false;
    public bool skipTurn = false;

    public bool waitingForDestroy = false;
    public bool takingDamage = false;
    private void Start()
    {
        InitializeVariables();
        InitializeStats();
    }

    void InitializeVariables()
    {
        selectionCamera = CameraManager.instance.selectCamera;
        statusMenu = transform.Find("Status").gameObject.GetComponent<Canvas>();

        healthBar = statusMenu.transform.Find("Health Bar").gameObject.GetComponentInChildren<Slider>();

        if (isPlayerControlled)
        {
            actionCamera = transform.Find("ActionCamera").gameObject.GetComponent<CinemachineVirtualCamera>();
            actionCamera.LookAt = GameObject.Find("ENEMYSIDE").transform;


            battleMenu = transform.Find("MainSelection").gameObject.GetComponent<Canvas>();
            abilityMenu = transform.Find("Abilities").gameObject.GetComponent<Canvas>();

            attackButton = battleMenu.GetComponentsInChildren<Button>(true)[0];
            runButton = battleMenu.GetComponentsInChildren<Button>(true)[2];
            abilityButtons = abilityMenu.GetComponentsInChildren<Button>(true);
        }
        else
        {
            nameText = statusMenu.transform.Find("Panel").gameObject;
            nameText.gameObject.SetActive(true);
            nameText.GetComponentInChildren<TextMeshProUGUI>(true).text = name;
            nameText.GetComponentInChildren<TextMeshProUGUI>(true).gameObject.SetActive(true);


        }
    }
    private void InitializeStats()
    {
        maxHp = constitution * level + 1;
        attack = (int)(strength / 5f * level + 1);
        defense = (int)(constitution / 5f * level + 1);
        speed = (int)(dexterity / 5f * level + 1);

        if (isPlayerControlled) currentHp = PlayerData.teamData.Find(item => item.id == id).currentHp;
        else currentHp = maxHp;

        if (currentHp < maxHp)
        {
            if (healthBar)
            {
                healthBar.gameObject.SetActive(true);
                healthBar.value = (float)currentHp / maxHp;
            }
        }
    }
    public bool ActivateCamera()
    {
        if (actionCamera == null) return false;

        actionCamera.gameObject.SetActive(true);
        return true;
    }
    public void DeactivateCamera()
    {
        if (actionCamera == null) return;
        actionCamera.gameObject.SetActive(false);
    }

    public bool SelectTarget(GameObject target)
    {
        if (selectionCamera == null) return false;

        selectionCamera.LookAt = target.transform;
        CameraManager.instance.ActivateSelectionCamera();
        return true;
    }

    public void EndSelect()
    {
        if (selectionCamera == null) return;
        selectionCamera.gameObject.SetActive(false);
    }
    public void OpenBattleMenu()
    {
        if (battleMenu == null) return;
        battleMenu.gameObject.SetActive(true);

        attackButton.onClick.AddListener(delegate { TBBS.instance.AbilityMenu(this); });
        runButton.onClick.AddListener(delegate { TBBS.instance.Run(this); });
    }
    public void CloseBattleMenu()
    {
        if (battleMenu == null) return;
        battleMenu.gameObject.SetActive(false);

        attackButton.onClick.RemoveAllListeners();
        runButton.onClick.RemoveAllListeners();
    }

    public void OpenAbilityMenu()
    {
        if (abilityMenu == null) return;

        abilityMenu.gameObject.SetActive(true);

        for (int i = 0; i < knownAbilities.Length; i++)
        {
            abilityButtons[i].gameObject.SetActive(true);
            abilityButtons[i].interactable = true;
            abilityButtons[i].GetComponentInChildren<TMP_Text>().text = knownAbilities[i].name;
            int index = i;

            abilityButtons[index].onClick.AddListener(delegate { TBBS.instance.SelectAbility(knownAbilities[index]); });

            if (knownAbilities[i].abilityType == AbilityType.PASSIVE) abilityButtons[i].interactable = false;

            if (knownAbilities[i].mustUseStance && currentStance != knownAbilities[i].stance) abilityButtons[i].interactable = false;
        }
    }

    public void CloseAbilityMenu()
    {
        if (abilityMenu == null) return;
        abilityMenu.gameObject.SetActive(false);

        for (int i = 0; i < knownAbilities.Length; i++)
        {
            int index = i;
            abilityButtons[index].onClick.RemoveAllListeners();
        }
    }

    public void ApplyStatModifier(Stats stat, float mod)
    {
        if (mod > 1) VFXManager.instance.SpawnGlobalEffect(VFX.BUFF, gameObject);
        else VFXManager.instance.SpawnGlobalEffect(VFX.NERF, gameObject);

        switch (stat)
        {
            case Stats.ATK:
                attack = Mathf.FloorToInt(attack * mod);
                break;
            case Stats.DEF:
                defense = Mathf.FloorToInt(defense * mod);
                break;
            case Stats.SPEED:
                speed = Mathf.FloorToInt(speed * mod);
                break;
            case Stats.LUCK:
                luck = Mathf.FloorToInt(luck * mod);
                break;
            default:
                break;
        }
    }

    public void RemoveStatModifier(Stats stat)
    {
        switch (stat)
        {
            case Stats.ATK:
                attack = (int)(strength / 5f * level + 1);
                break;
            case Stats.DEF:
                defense = (int)(constitution / 5f * level + 1);
                break;
            case Stats.SPEED:
                speed = (int)(dexterity / 5f * level + 1);
                break;
            case Stats.LUCK:
                break;
            default:
                break;
        }
    }

    public void Heal(int healAmount)
    {
        if (currentHp + healAmount >= maxHp) currentHp = maxHp;
        else currentHp = currentHp + healAmount;

        StartCoroutine(UpdateHealthBar());
    }

    public void TakeDamage(int dmgAmount)
    {
        if (currentHp - dmgAmount <= 0)
        {
            currentHp = 0;
            waitingForDestroy = true;
        }
        else
        {
            currentHp = currentHp - dmgAmount;
        }
        StartCoroutine(UpdateHealthBar());
    }

    IEnumerator UpdateHealthBar()
    {
        takingDamage = true;

        if (!healthBar)
        {
            if (currentHp == 0) TBBS.instance.Death(this);
            yield break;
        }

        healthBar.gameObject.SetActive(true);
        float t = 0;
        float startValue = healthBar.value;
        float endValue = currentHp / maxHp;

        if (endValue > startValue) FresnelApplier.applyFresnel(gameObject, UnityEngine.Color.lightGreen);
        else FresnelApplier.applyFresnel(gameObject, UnityEngine.Color.red);

        while (t < 1)
        {
            healthBar.value = Mathf.Lerp(startValue, endValue, t);
            t += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(.3f);

        //healthBar.gameObject.SetActive(false);

        if (status != Status.NONE) VFXManager.instance.SpawnStatusVFX(status, gameObject);
        else FresnelApplier.clearFresnel(gameObject);

        takingDamage = false;

        if (currentHp == 0) TBBS.instance.Death(this);
    }

    public int GetStat(Stats stat)
    {
        switch (stat)
        {
            case Stats.ATK:
                int atk = attack;
                if (HasPassive("Double Trouble")) atk = Mathf.FloorToInt(atk * .5f);
                if (status == Status.BURNED)
                {
                    atk = HasPassive("Piromaniac") ? atk : Mathf.FloorToInt(atk * .5f);
                }
                if (currentStance == Stance.AGRESSIVE) return Mathf.FloorToInt(atk * stanceModifier);
                else return atk;
            case Stats.DEF:
                if (currentStance == Stance.DEFENSIVE) return Mathf.FloorToInt(defense * stanceModifier);
                else return defense;
            case Stats.SPEED:
                int spd = status == Status.PARALYZED ? Mathf.FloorToInt(speed * .5f) : speed;
                if (currentStance == Stance.AGILE) return Mathf.FloorToInt(spd * stanceModifier);
                else return spd;
            case Stats.LUCK:
                return luck;
            default:
                return 0;
        }
    }

    public void OnBattleStart()
    {
        ResolvePassiveEffect(PassiveExecutionTime.BATTLESTART);
    }

    public void OnTurnStart()
    {
        ResolvePassiveEffect(PassiveExecutionTime.TURNSTART);
    }

    public void OnTurnEnd()
    {
        switch (status)
        {
            case Status.NONE:
                break;
            case Status.BURNED:
                TakeDamage((Mathf.FloorToInt(maxHp * .1f) + 1));
                Debug.Log(name + " lost " + (Mathf.FloorToInt(maxHp * .1f) + 1) + " hp due to his burns");
                break;
            case Status.POISONED:
                TakeDamage((Mathf.FloorToInt(maxHp * .2f) + 1));
                Debug.Log(name + " lost " + (Mathf.FloorToInt(maxHp * .2f) + 1) + " hp due to poison");
                break;
            default:
                break;
        }

        ResolvePassiveEffect(PassiveExecutionTime.TURNEND);
    }

    public void ResolvePassiveEffect(PassiveExecutionTime battleStage, Unit lastHitUnit = null)
    {
        foreach (var item in knownAbilities)
        {
            if (item.abilityType == AbilityType.PASSIVE && item.passiveExecutionTime == battleStage && item.passiveEffectChance >= Random.Range(1, 100))
            {
                List<Unit> target = new List<Unit>();

                switch (item.target)
                {
                    case AbilityTarget.SELF:
                        target.Add(this);
                        break;
                    case AbilityTarget.ONEALLY:
                        if (lastHitUnit) target.Add(lastHitUnit);
                        else target.Add(TBBS.instance.playerUnits[Random.Range(0, TBBS.instance.playerUnits.Count)]);
                        break;
                    case AbilityTarget.ONEENEMY:
                        if (lastHitUnit) target.Add(lastHitUnit);
                        else target.Add(TBBS.instance.enemyUnits[Random.Range(0, TBBS.instance.enemyUnits.Count)]);
                        break;
                    case AbilityTarget.ALLALLIES:
                        target.AddRange(TBBS.instance.playerUnits);
                        break;
                    case AbilityTarget.ALLENEMIES:
                        target.AddRange(TBBS.instance.enemyUnits);
                        break;
                    case AbilityTarget.ALL:
                        target.AddRange(TBBS.instance.allUnits);
                        break;
                }
                switch (item.passiveEffects)
                {
                    case PassiveEffects.UPATK:
                        foreach (var unit in target)
                        {
                            Debug.Log(unit.name + " attack raises!");
                            unit.ApplyStatModifier(Stats.ATK, 1.5f);
                        }
                        break;
                    case PassiveEffects.UPATKONSTATUS:
                        foreach (var unit in target)
                        {
                            if (unit.status != item.status) continue;
                            Debug.Log(unit.name + " attack raises!");
                            unit.ApplyStatModifier(Stats.ATK, 1.5f);
                        }
                        break;
                    case PassiveEffects.UPDEF:
                        foreach (var unit in target)
                        {
                            Debug.Log(unit.name + " defense raises!");
                            unit.ApplyStatModifier(Stats.DEF, 1.5f);
                        }
                        break;
                    case PassiveEffects.UPSPEED:
                        foreach (var unit in target)
                        {
                            Debug.Log(unit.name + " speed raises!");
                            unit.ApplyStatModifier(Stats.SPEED, 1.5f);
                        }
                        break;
                    case PassiveEffects.DOWNATK:
                        foreach (var unit in target)
                        {
                            Debug.Log(unit.name + " attack fell!");
                            unit.ApplyStatModifier(Stats.ATK, .75f);
                        }
                        break;
                    case PassiveEffects.DOWNDEF:
                        foreach (var unit in target)
                        {
                            Debug.Log(unit.name + " defense fell!");
                            unit.ApplyStatModifier(Stats.DEF, .75f);
                        }
                        break;
                    case PassiveEffects.DOWNSPEED:
                        foreach (var unit in target)
                        {
                            Debug.Log(unit.name + " speed fell!");
                            unit.ApplyStatModifier(Stats.SPEED, .75f);
                        }
                        break;
                    case PassiveEffects.ADDTURN:
                        additionalTurn = true;
                        break;
                    case PassiveEffects.SKIPTURN:
                        Debug.Log(name + " is slacking.");
                        skipTurn = true;
                        break;
                    case PassiveEffects.APPLYSTATUS:
                        foreach (var unit in target)
                        {
                            unit.ApplyStatus(item.status);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void ApplyStatus(Status statusToApply)
    {
        if (status != Status.NONE) return;

        if (!takingDamage) VFXManager.instance.SpawnStatusVFX(statusToApply, gameObject);
        else
        {
            StartCoroutine(WaitToApplyStatus(statusToApply));
            return;
        }

        status = statusToApply;

        ResolvePassiveEffect(PassiveExecutionTime.ONSTATUSCHANGE);
    }
    IEnumerator WaitToApplyStatus(Status statusToApply)
    {
        yield return new WaitForSeconds(.1f);
        ApplyStatus(statusToApply);
        yield break;
    }

    public void CureStatus()
    {
        if (status == Status.NONE) return;

        status = Status.NONE;
        FresnelApplier.clearFresnel(gameObject);

        ResolvePassiveEffect(PassiveExecutionTime.ONSTATUSCHANGE);
    }

    public bool HasAdditionalTurn()
    {
        if (!additionalTurn) return false;

        additionalTurn = false;

        return true;
    }

    public bool HasPassive(string passiveName)
    {
        foreach (var item in knownAbilities)
        {
            if (item.abilityType == AbilityType.ACTIVE) continue;

            if (item.name == passiveName) return true;
        }
        return false;
    }
}
