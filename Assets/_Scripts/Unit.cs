using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using GameData;
using TMPro;
using System.Collections.Generic;
using System.Collections;
public class Unit : MonoBehaviour
{
    [Header("Properties")]
    new public string name;
    public string description;
    public Stance[] stances;
    public Stance currentStance;
    public int level;
    public Status status;
    private float stanceModifier = 1.5f;

    [Header("Base Stats")]
    public int strength;
    public int constitution;
    public int dexterity;
    public int luck;

    [Header("Stats")]
    [SerializeField]
    private int maxHp;
    private int currentHp;
    [SerializeField]
    private int attack;
    [SerializeField]
    private int defense;
    [SerializeField]
    private int speed;

    [Header("Abilities")]
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
    private Button attackButton;
    [SerializeField]
    private Button runButton;
    [SerializeField]
    private Button[] abilityButtons;
    [SerializeField]
    private Slider healthBar;
    [SerializeField]
    private GameObject nameText;

    private bool additionalTurn = false;
    public bool skipTurn = false;

    public bool waitingForDestroy = false;

    private void Awake()
    {
        InitializeStats();

        if (nameText) nameText.GetComponent<TextMeshProUGUI>().text = name;

        if (actionCamera) actionCamera.LookAt = GameObject.Find("ENEMYSIDE").transform;

        selectionCamera = CameraManager.instance.selectCamera;
    }
    private void InitializeStats()
    {
        maxHp = constitution * level + 1;
        attack = (int)(strength / 5f * level + 1);
        defense = (int)(constitution / 5f * level + 1);
        speed = (int)(dexterity / 5f * level + 1);

        currentHp = maxHp;
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
        if(battleMenu == null) return;
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
        switch (stat)
        {
            case Stats.ATK:
                attack = Mathf.FloorToInt(attack*mod);
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
        if(currentHp + healAmount >= maxHp) currentHp = maxHp;
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
        else currentHp = currentHp - dmgAmount;

        StartCoroutine(UpdateHealthBar());
    }

    IEnumerator UpdateHealthBar()
    {
        if(!healthBar) yield break;

        healthBar.gameObject.SetActive(true);
        float t = 0;
        float startValue = healthBar.value;

        while (t < 1) 
        {
            healthBar.value = Mathf.Lerp(startValue, (float)currentHp/maxHp, t);
            t += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(.3f);

        healthBar.gameObject.SetActive(false);

        if (currentHp == 0) TBBS.instance.Death(this);
    }

    public int GetStat(Stats stat)
    {
        switch (stat)
        {
            case Stats.ATK:
                int atk = status == Status.BURNED ? Mathf.FloorToInt(attack *.5f) : attack;
                if (currentStance == Stance.AGRESSIVE) return Mathf.FloorToInt(atk * stanceModifier);
                else return atk;
            case Stats.DEF:
                if (currentStance == Stance.DEFFENSIVE) return Mathf.FloorToInt(defense * stanceModifier);
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
                Debug.Log(name + " lost " + (Mathf.FloorToInt(currentHp * .1f) + 1) + " hp due to his burns");
                break;
            case Status.POISONED:
                Debug.Log(name + " lost " + (Mathf.FloorToInt(currentHp * .1f) + 1) + " hp due to poison");
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
            if(item.abilityType == AbilityType.PASSIVE && item.passiveExecutionTime == battleStage)
            {
                List<Unit> target = new List<Unit>();

                switch (item.target)
                {
                    case AbilityTarget.SELF:
                        target.Add(this);
                        break;
                    case AbilityTarget.ONEALLY:
                        if(lastHitUnit) target.Add(lastHitUnit);
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
                        ApplyStatModifier(Stats.ATK, .5f);
                        break;
                    case PassiveEffects.SKIPTURN:
                        if (item.passiveEffectChance >= Random.Range(1, 100))
                        {
                            Debug.Log(name + " is slacking.");
                            skipTurn = true;
                        }
                        else skipTurn = false;
                        break;
                    case PassiveEffects.APPLYBURN:
                        if (item.passiveEffectChance >= Random.Range(1, 100))
                        {
                            foreach (var unit in target)
                            { 
                                unit.ApplyStatus(Status.BURNED);
                            }
                        }
                        break;
                    case PassiveEffects.APPLYPARA:
                        if (item.passiveEffectChance >= Random.Range(1, 100))
                        {
                            foreach (var unit in target)
                            {
                                unit.ApplyStatus(Status.PARALYZED);
                            }
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
        
        status = statusToApply;
    }

    public void CureStatus()
    {
        status = Status.NONE;
    }

    public bool HasAdditionalTurn()
    {
        foreach (var item in knownAbilities)
        {
            if (item.abilityType == AbilityType.PASSIVE && item.passiveEffects == PassiveEffects.ADDTURN && !additionalTurn)
            {
                Debug.Log(name + " gains an extra turn!");
                additionalTurn = true;
                return true;
            }
        }

        Debug.Log("No add turn ability");
        additionalTurn = false;
        return false;
    }
}
