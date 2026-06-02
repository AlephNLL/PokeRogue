using Cinemachine;
using GameData;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Unit : MonoBehaviour
{
    public Sprite icon;
    public int id;
    new public string name;
    public string description;
    public Stance currentStance;
    public int level;
    public int exp;
    public ExpCurve expCurve;
    public Status status;
    private float stanceModifier = 1.5f;

    public float recivedDamageMultiplier = 1f;

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

    public float precision = 1;

    public Abilities[] knownAbilities;
    public Abilities[] abilityPool;

    public Item heldItem;

    public bool evasive = false;
    public bool provoking = false;
    public Unit guardedBy = null;

    public int timesBuffed;

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
    private Canvas itemMenu;
    [SerializeField]
    public Canvas statusMenu;
    [SerializeField]
    public Button attackButton;
    [SerializeField]
    private Button itemButton;
    [SerializeField]
    private Button runButton;
    [SerializeField]
    private Button[] abilityButtons;
    [SerializeField]
    private Button[] itemButtons;
    [SerializeField]
    public Slider healthBar;
    [SerializeField]
    private GameObject nameText;

    public bool isPlayerControlled = false;

    public bool additionalTurn = false;
    public bool skipTurn = false;

    public bool takingDamage = false;

    int trickyStanceEffectChanceModifier = 30;
    public float baseEffectChanceMulti = 1;
    public int effectChanceModifier = 0;

    int sleepCounter;
    int sleepMaxTurns = 3;

    GameObject lastMenu;
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

        recivedDamageMultiplier = 1f;

        if (isPlayerControlled)
        {
            actionCamera = transform.Find("ActionCamera").gameObject.GetComponent<CinemachineVirtualCamera>();
            actionCamera.LookAt = GameObject.Find("ENEMYSIDE").transform;


            battleMenu = transform.Find("MainSelection").gameObject.GetComponent<Canvas>();
            abilityMenu = transform.Find("Abilities").gameObject.GetComponent<Canvas>();
            itemMenu = transform.Find("Items").gameObject.GetComponent<Canvas>();

            attackButton = battleMenu.GetComponentsInChildren<Button>(true)[0];
            itemButton = battleMenu.GetComponentsInChildren<Button>(true)[1];
            runButton = battleMenu.GetComponentsInChildren<Button>(true)[2];
            runButton.GetComponentInChildren<TMP_Text>().text = "Defend";
            abilityButtons = abilityMenu.GetComponentsInChildren<Button>(true);
            itemButtons = itemMenu.GetComponentsInChildren<Button>(true);
        }
        else
        {
            nameText = statusMenu.transform.Find("Panel").gameObject;
            nameText.gameObject.SetActive(true);
            nameText.GetComponentInChildren<TextMeshProUGUI>(true).text = $"{name} Lvl: {BattleData.enemyLevel}";
            nameText.GetComponentInChildren<TextMeshProUGUI>(true).gameObject.SetActive(true);
        }
    }
    public int GetRawStat(Stats stat, int monLevel)
    {
        switch (stat)
        {
            case Stats.HP:
                return (int)(constitution * monLevel/20f + monLevel + 5);
            case Stats.ATK:
                return (int)(strength * monLevel/ 20f + 5);
            case Stats.DEF:
                return (int)((.5f * constitution + .5f * dexterity) * monLevel/ 20f + 5);
            case Stats.SPEED:
                return (int)(dexterity * monLevel/ 20f + 5);
            default:
                return 0;
        }
    }
    private void InitializeStats()
    {
        if (isPlayerControlled)
        {
            currentHp = PlayerData.teamData.Find(item => item.id == id).currentHp;
            level = PlayerData.teamData.Find(item => item.id == id).level;

            maxHp = GetRawStat(Stats.HP, level);
            attack = GetRawStat(Stats.ATK, level);
            defense = GetRawStat(Stats.DEF, level);
            speed = GetRawStat(Stats.SPEED, level);

            knownAbilities = PlayerData.teamData.Find(item => item.id == id).knownAbilities.ToArray();
            ApplyStatus(PlayerData.teamData.Find(item => item.id == id).status);
            heldItem = PlayerData.teamData.Find(item => item.id == id).heldItem;
        }
        else
        {
            level = BattleData.enemyLevel;

            maxHp = GetRawStat(Stats.HP, level);
            attack = GetRawStat(Stats.ATK, level);
            defense = GetRawStat(Stats.DEF, level);
            speed = GetRawStat(Stats.SPEED, level);

            currentHp = maxHp;
            knownAbilities = GetUnitKnownAbilities(level).ToArray();
        }



        if (currentHp < maxHp)
        {
            if (healthBar)
            {
                healthBar.gameObject.SetActive(true);
                healthBar.value = (float)currentHp / maxHp;
            }
        }

        TooltipUI.instance.EndCurrentAction();
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
        itemButton.onClick.AddListener(delegate { TBBS.instance.ItemMenu(this); });
        runButton.onClick.AddListener(delegate { TBBS.instance.Skip(this); });

        lastMenu = battleMenu.gameObject;
    }
    public void CloseBattleMenu()
    {
        if (battleMenu == null) return;
        battleMenu.gameObject.SetActive(false);

        attackButton.onClick.RemoveAllListeners();
        runButton.onClick.RemoveAllListeners();

        lastMenu = null;
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

        lastMenu = abilityMenu.gameObject;
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

        lastMenu = null;
    }

    public void OpenItemMenu()
    {
        if (itemMenu == null) return;
        itemMenu.gameObject.SetActive(true);

        List<Item> consumables = new List<Item>();
        for (int i = 0; i < PlayerData.items.Count; i++)
        {
            if (PlayerData.items[i].isConsumible)
            {
                consumables.Add(PlayerData.items[i]);
            }
        }

        for (int i = 0; i < consumables.Count; i++)
        {
            itemButtons[i].gameObject.SetActive(true);
            itemButtons[i].interactable = true;
            itemButtons[i].GetComponentInChildren<TMP_Text>().text = consumables[i].name;

            int index = i;

            itemButtons[index].onClick.AddListener(delegate { TBBS.instance.SelectItem(consumables[index]); });
        }

        lastMenu = itemMenu.gameObject;
    }

    public void CloseItemMenu()
    {
        if (itemMenu == null) return;
        itemMenu.gameObject.SetActive(false);

        for (int i = 0; i < itemButtons.Length; i++)
        {
            int index = i;
            itemButtons[index].onClick.RemoveAllListeners();
            itemButtons[index].gameObject.SetActive(false);
        }

        lastMenu = null;
    }

    public void ApplyStatModifier(Stats stat, float mod, bool contaged = false)
    {
        if (mod > 1) VFXManager.instance.SpawnGlobalEffect(VFX.BUFF, gameObject);
        else VFXManager.instance.SpawnGlobalEffect(VFX.NERF, gameObject);

        string modAction = mod > 1 ? "rose" : "fell";

        if (HasPassive("Double Or Nothing")) mod = mod > 1 ? mod * 2 : mod / 2;

        switch (stat)
        {
            case Stats.ATK:
                attack = Mathf.FloorToInt(attack * mod);
                TooltipUI.instance.AddEffectToCurrentAction($"{name}'s attack {modAction}");
                break;
            case Stats.DEF:
                defense = Mathf.FloorToInt(defense * mod);
                TooltipUI.instance.AddEffectToCurrentAction($"{name}'s defense {modAction}");
                break;
            case Stats.SPEED:
                speed = Mathf.FloorToInt(speed * mod);
                TooltipUI.instance.AddEffectToCurrentAction($"{name}'s speed {modAction}");
                break;
            case Stats.LUCK:
                luck = Mathf.FloorToInt(luck * mod);
                TooltipUI.instance.AddEffectToCurrentAction($"{name}'s luck {modAction}");
                break;
            case Stats.PRECISION:
                precision = precision * mod;
                TooltipUI.instance.AddEffectToCurrentAction($"{name}'s precision {modAction}");
                break;
            case Stats.EFFECTCHANCEMOD:
                baseEffectChanceMulti = baseEffectChanceMulti * mod;
                TooltipUI.instance.AddEffectToCurrentAction($"{name}'s abilities secondary effect chance {modAction}");
                break;
            default:
                break;
        }

        if (!contaged && HasPassive("Contagious"))
        {
            List<Unit> allies = new List<Unit>(TBBS.instance.playerUnits);
            allies.Remove(this);
            TooltipUI.instance.EndCurrentAction();
            TooltipUI.instance.StartNewAction($"{name} shares stat changes");
            foreach (Unit ally in allies)
            {
                ally.ApplyStatModifier(stat, mod, true);
            }
        }
    }

    public void IncreaseStat(Stats stat, int amount)
    {
        switch (stat)
        {
            case Stats.HP:
                break;
            case Stats.ATK:
                attack += amount;
                break;
            case Stats.DEF:
                defense += amount;
                break;
            case Stats.SPEED:
                speed += amount;
                break;
            case Stats.LUCK:
                luck += amount;
                break;
            case Stats.PRECISION:
                break;
            case Stats.EFFECTCHANCEMOD:
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
    public void SwapStats(Stats statA, Stats statB)
    {
        int tempValue = GetSetStat(statA);

        GetSetStat(statA, GetSetStat(statB));

        GetSetStat(statB, tempValue);

        // Opcional: Feedback visual o logs
        VFXManager.instance.SpawnGlobalEffect(VFX.BUFF, gameObject); // O un efecto de "espejo/cambio"
        TooltipUI.instance.AddEffectToCurrentAction($"{name} swapped {statA} and {statB}!");

        Debug.Log($"{name} intercambió {statA} por {statB}. Nuevos valores -> {statA}: {GetSetStat(statA)} | {statB}: {GetSetStat(statB)}");
    }
    public int GetSetStat(Stats stat, int? newValue = null)
    {
        switch (stat)
        {
            case Stats.ATK:
                if (newValue.HasValue) attack = newValue.Value;
                return attack;
            case Stats.DEF:
                if (newValue.HasValue) defense = newValue.Value;
                return defense;
            case Stats.SPEED:
                if (newValue.HasValue) speed = newValue.Value;
                return speed;
            case Stats.LUCK:
                if (newValue.HasValue) luck = newValue.Value;
                return luck;
            default:
                return 0;
        }
    }
    public void ChangeStance(Stance stance)
    {
        currentStance = stance;
        FresnelApplier.changeStance(this.gameObject, currentStance);
        if (currentStance == Stance.TRICKY) effectChanceModifier = trickyStanceEffectChanceModifier;
        else effectChanceModifier = 0;

        TooltipUI.instance.AddEffectToCurrentAction($"{name} changes to a {stance.ToString().ToLower()} stance");
    }

    public void GetInitialStance(Stance stance)
    {
        currentStance = stance;
        FresnelApplier.changeStance(this.gameObject, currentStance);
        if (currentStance == Stance.TRICKY) effectChanceModifier = trickyStanceEffectChanceModifier;
        else effectChanceModifier = 0;
    }

    public void Heal(int healAmount)
    {
        if (currentHp >= maxHp) return;

        currentHp = currentHp + healAmount;

        if (currentHp > maxHp) currentHp = maxHp;

        StartCoroutine(UpdateHealthBar());
    }
    public void HealPercent(float healPercent)
    {
        if (currentHp >= maxHp) return;

        currentHp = (int)(currentHp + maxHp * healPercent);

        if (currentHp > maxHp) currentHp = maxHp;

        StartCoroutine(UpdateHealthBar());
    }

    public void TakeDamage(int dmgAmount)
    {
        if (dmgAmount == 0) return;
        if (currentHp - dmgAmount <= 0)
        {
            currentHp = 0;
            TBBS.instance.WaitingForDestroy(this);
            LeftHandAnimatorHelper.instance.TryFlick(this);
        }
        else
        {
            currentHp = currentHp - dmgAmount;
        }
        StartCoroutine(UpdateHealthBar());
    }

    public void TakeDamagePercent(float dmgPercent)
    {
        currentHp = (int)(currentHp - maxHp * dmgPercent);

        if (currentHp <= 0)
        {
            currentHp = 0;
            TBBS.instance.WaitingForDestroy(this);
            LeftHandAnimatorHelper.instance.TryFlick(this);
        }

        StartCoroutine(UpdateHealthBar());
    }

    IEnumerator UpdateHealthBar()
    {
        takingDamage = true;

        if (!healthBar)
        {
            if (currentHp == 0) Death();
            yield break;
        }

        healthBar.gameObject.SetActive(true);
        float t = 0;
        float startValue = healthBar.value;
        float endValue = (float)currentHp / maxHp;

        if (startValue == endValue) yield break;

        if (endValue > startValue) { FresnelApplier.applyFresnel(gameObject, UnityEngine.Color.lightGreen); VFXManager.instance.SpawnGlobalEffect(VFX.HEAL, gameObject); }
        else { FresnelApplier.applyFresnel(gameObject, UnityEngine.Color.red); VFXManager.instance.SpawnGlobalEffect(VFX.HIT, gameObject); }

        while (t < 1)
        {
            healthBar.value = Mathf.Lerp(startValue, endValue, t);
            t += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(.3f);

        if (status != Status.NONE) VFXManager.instance.SpawnStatusFresnelOnly(status, gameObject);
        else FresnelApplier.clearFresnel(gameObject);

        takingDamage = false;

        //if (currentHp == 0)
        //{
        //    Death();
        //}
    }

    public void Death()
    {
        ResolvePassiveEffect(ExecutionTime.ONDEATH);
        Destroy(gameObject);
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
                    atk = HasPassive("Pyromaniac") ? atk : Mathf.FloorToInt(atk * .5f);
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
        ResolvePassiveEffect(ExecutionTime.BATTLESTART);
        ResolveItemEffect(ExecutionTime.BATTLESTART);
        GetInitialStance(currentStance);
    }

    public void OnTurnStart()
    {
        ResolvePassiveEffect(ExecutionTime.TURNSTART);
        ResolveItemEffect(ExecutionTime.TURNSTART);

        if (status == Status.ASLEEP) SleepCounter();

        recivedDamageMultiplier = 1f;
    }

    public void OnTurnEnd()
    {
        switch (status)
        {
            case Status.NONE:
                break;
            case Status.BURNED:
                TakeDamage((Mathf.FloorToInt(maxHp * .1f) + 1));
                TooltipUI.instance.StartNewAction(name + " lost " + (Mathf.FloorToInt(maxHp * .1f) + 1) + " hp due to his burns");
                break;
            case Status.POISONED:
                TakeDamage((Mathf.FloorToInt(maxHp * .2f) + 1));
                TooltipUI.instance.StartNewAction(name + " lost " + (Mathf.FloorToInt(maxHp * .2f) + 1) + " hp due to poison");
                break;
            default:
                break;
        }

        TooltipUI.instance.EndCurrentAction();
        ResolvePassiveEffect(ExecutionTime.TURNEND);
        ResolveItemEffect(ExecutionTime.TURNEND);
    }
    public void OnRoundEnd()
    {
        provoking = false;
        guardedBy = null;
        baseEffectChanceMulti = 1;
    }
    public void ResolvePassiveEffect(ExecutionTime battleStage, Unit lastHitUnit = null)
    {
        foreach (var ability in knownAbilities)
        {
            if (ability.abilityType == AbilityType.PASSIVE && ability.passiveExecutionTime == battleStage && baseEffectChanceMulti * ability.passiveEffectChance + effectChanceModifier >= Random.Range(1, 100))
            {
                List<Unit> target = new List<Unit>();
                List<Unit> allies = isPlayerControlled ? new List<Unit>(TBBS.instance.playerUnits) : new List<Unit>(TBBS.instance.enemyUnits);
                allies.Remove(this);
                List<Unit> enemies = isPlayerControlled ? new List<Unit>(TBBS.instance.enemyUnits) : new List<Unit>(TBBS.instance.playerUnits);

                if (allies.Count < 0) continue;

                switch (ability.target)
                {
                    case AbilityTarget.SELF:
                        target.Add(this);
                        break;
                    case AbilityTarget.ONEALLY:
                        target.Add(allies[Random.Range(0, allies.Count)]);
                        break;
                    case AbilityTarget.ONEENEMY:
                        target.Add(enemies[Random.Range(0, enemies.Count)]);
                        break;
                    case AbilityTarget.ALLALLIES:
                        target.AddRange(allies);
                        break;
                    case AbilityTarget.ALLENEMIES:
                        target.AddRange(TBBS.instance.enemyUnits);
                        break;
                    case AbilityTarget.ALL:
                        target.AddRange(TBBS.instance.allUnits);
                        break;
                    case AbilityTarget.LASTHIT:
                        if (lastHitUnit) target.Add(lastHitUnit);
                        break;
                    case AbilityTarget.EXECUTIONER:
                        target.Add(TBBS.instance.allUnits[TBBS.instance.currentTurnIndex]);
                        break;
                    default:
                        break;
                }
                switch (ability.condition1)
                {
                    case AbilityCondition.NONE:
                        break;
                    case AbilityCondition.ISBURNED:
                        if (status != Status.BURNED) continue;
                        break;
                    case AbilityCondition.ISASLEEP:
                        if (status != Status.ASLEEP) continue;
                        break;
                    default:
                        break;
                }

                if(ability.passiveExecutionTime != ExecutionTime.ONHIT) TooltipUI.instance.StartNewAction($"{name}' ability {ability.name} activates!");
                else TooltipUI.instance.AddEffectToCurrentAction($"{name}' ability {ability.name} activates!");

                switch (ability.passiveEffects)
                    {
                        case PassiveEffects.STATMOD:
                            for (int i = 0; i < ability.statToMod.Length; i++)
                            {
                                for (int j = 0; j < target.Count; j++)
                                {
                                    target[j].ApplyStatModifier(ability.statToMod[i], ability.statMod[i]);
                                }
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
                                unit.ApplyStatus(ability.status);
                            }
                            break;
                        case PassiveEffects.HEAL:
                            foreach (var unit in target)
                            {
                                unit.HealPercent(ability.healPercentage);
                            }
                            break;
                        case PassiveEffects.STACKSTAT:
                            TooltipUI.instance.AddEffectToCurrentAction($"{name} shares stats with the team!");
                            for (int i = 0; i < ability.statToMod.Length; i++)
                            {
                                for (int j = 0; j < target.Count; j++)
                                {
                                    target[j].IncreaseStat(ability.statToMod[i], GetStat(ability.statToMod[i]));
                                }
                            }
                            break;
                        case PassiveEffects.DAMAGE:
                            foreach (var unit in target)
                            {
                                unit.TakeDamagePercent(ability.healPercentage);
                            }
                            break;
                        default:
                            break;
                    }

                TooltipUI.instance.EndCurrentAction();
            }
        }
    }

    public void ResolveItemEffect(ExecutionTime battleStage, Unit lastHitUnit = null)
    {
        if (!heldItem) return;

        Unit target = heldItem.affectSelf ? this : lastHitUnit;

        for (int i = 0; i < heldItem.effect.Length; i++)
        {
            if (heldItem.executionTime[i] == battleStage && baseEffectChanceMulti * heldItem.effectChance[i] + effectChanceModifier >= Random.Range(1, 100))
            {
                switch (heldItem.condition[i])
                {
                    case AbilityCondition.NONE:
                        break;
                    case AbilityCondition.ISBURNED:
                        if (status != Status.BURNED) continue;
                        break;
                    case AbilityCondition.ISASLEEP:
                        if (status != Status.ASLEEP) continue;
                        break;
                    case AbilityCondition.ISFROZEN:
                        if (status != Status.FROZEN) continue;
                        break;
                    case AbilityCondition.ISPARALYZED:
                        if (status != Status.PARALYZED) continue;
                        break;
                    default:
                        break;
                }

                if (heldItem.executionTime[i] == ExecutionTime.BATTLESTART || heldItem.executionTime[i] == ExecutionTime.TURNSTART) TooltipUI.instance.StartNewAction($"{name}' item {heldItem.name} activates!");
                else TooltipUI.instance.AddEffectToCurrentAction($"{name}' item {heldItem.name} activates!");

                switch (heldItem.effect[i])
                {
                    case ItemEffects.STATMOD:
                        for (int j = 0; j < heldItem.statToMod.Length; j++)
                        {
                            target.ApplyStatModifier(heldItem.statToMod[j], heldItem.statMod[j]);
                        }
                        break;
                    case ItemEffects.ADDTURN:
                        target.additionalTurn = true;
                        break;
                    case ItemEffects.APPLYSTATUS:
                        target.ApplyStatus(heldItem.statusToChangeTo);
                        break;
                    case ItemEffects.CURESTATUS:
                        target.CureStatus();
                            break;
                    case ItemEffects.FLINCH:
                        target.skipTurn = true; 
                        break;
                    case ItemEffects.INCREASESTAT:
                        for (int j = 0; j < heldItem.statToMod.Length; j++)
                        {
                            target.IncreaseStat(heldItem.statToMod[j], (int)heldItem.statMod[j]);
                        }
                        break;
                    default:
                        break;
                }

                if (heldItem.executionTime[i] == ExecutionTime.BATTLESTART || heldItem.executionTime[i] == ExecutionTime.TURNSTART) TooltipUI.instance.EndCurrentAction();
            }
        }
        
    }

    public void ApplyStatus(Status statusToApply)
    {
        if (statusToApply == Status.NONE) return;
        if (status != Status.NONE) return;

        if (HasPassive("Elusive Presence"))
        {
            TooltipUI.instance.AddEffectToCurrentAction($"{name}'s elusive presence blocks status changes");
            return;
        }
        VFXManager.instance.SpawnStatusVFX(statusToApply, gameObject);
        TooltipUI.instance.AddEffectToCurrentAction($"{name} is {statusToApply.ToString().ToLower()}");
        status = statusToApply;

        if (statusToApply == Status.ASLEEP) StartSleepCounter();

        ResolvePassiveEffect(ExecutionTime.ONSTATUSCHANGE);
        ResolveItemEffect(ExecutionTime.ONSTATUSCHANGE);
    }

    public void CureStatus()
    {
        if (status == Status.NONE) return;

        TooltipUI.instance.AddEffectToCurrentAction($"{name} is no longer {status.ToString().ToLower()}");
        if (status == Status.ASLEEP) { skipTurn = false; Debug.Log("Despertao"); }
        status = Status.NONE;
        VFXManager.instance.ClearStatusVFXPrefab(gameObject);
        FresnelApplier.clearFresnel(gameObject);

        ResolvePassiveEffect(ExecutionTime.ONSTATUSCHANGE);
        ResolveItemEffect(ExecutionTime.ONSTATUSCHANGE);
    }
    void StartSleepCounter()
    {
        sleepCounter = 0;
        skipTurn = true;
    }
    void SleepCounter()
    {
        if (sleepCounter >= sleepMaxTurns)
        {
            WakeUp();
            return;
        }
        if (33 + sleepCounter * 16 > Random.Range(1, 101))
        {
            WakeUp();
            return;
        }
        TooltipUI.instance.StartNewAction(name + " is fast asleep.");
        TooltipUI.instance.EndCurrentAction();
        skipTurn = true;
        sleepCounter++;
    }
    void WakeUp()
    {
        TooltipUI.instance.StartNewAction(name + " woke up!");
        CureStatus();
        skipTurn = false;
        TooltipUI.instance.EndCurrentAction();
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

    public List<Abilities> GetUnitKnownAbilities(int monLevel)
    {
        List<Abilities> abilityList = new List<Abilities>();

        for (int i = 0; i < monLevel + 1; i++)
        {
            if (i > 10) break;
            if (i < abilityPool.Length)
            {
                abilityList.Add(abilityPool[i]);

                if (i > 3)
                {
                    abilityList.RemoveAt(0);
                }
            }
        }

        return abilityList;
    }

    public List<Abilities> GetStanceLockedAbilities()
    {
        List<Abilities> abilityList = new List<Abilities>();

        for (int i = 0; i < knownAbilities.Length; i++)
        {
            if (knownAbilities[i].mustUseStance) abilityList.Add(knownAbilities[i]);
        }

        return abilityList;
    }

    public bool OpenLastMenu()
    {
        if(!lastMenu) return false;
        lastMenu.SetActive(true);
        return true;
    }

    public bool CloseLastMenu()
    {
        if (!lastMenu) return false;
        lastMenu.SetActive(false);
        return true;
    }

    public void RegisterUIButtons()
    {
        List<RectTransform> buttons = new List<RectTransform>();
        buttons.Add(attackButton.gameObject.GetComponent<RectTransform>());
        buttons.Add(itemButton.gameObject.GetComponent<RectTransform>());
        buttons.Add(runButton.gameObject.GetComponent<RectTransform>());

        if (knownAbilities[0].abilityType == AbilityType.PASSIVE)
        {
            buttons.Add(abilityButtons[1].gameObject.GetComponent<RectTransform>());
            buttons.Add(abilityButtons[0].gameObject.GetComponent<RectTransform>());
        }
        else
        {
            buttons.Add(abilityButtons[0].gameObject.GetComponent<RectTransform>());
            buttons.Add(abilityButtons[1].gameObject.GetComponent<RectTransform>());
        }
        BattleTutorialManager.instance.RegisterPlayerUI(buttons.ToArray());
    }
}
