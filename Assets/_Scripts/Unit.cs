using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using GameData;
using TMPro;
public class Unit : MonoBehaviour
{
    [Header("Properties")]
    new public string name;
    public string description;
    public Stance[] stances;
    public Stance currentStance;
    public int level;
    public Status status;

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

    private void Awake()
    {
        InitializeStats();

        actionCamera.LookAt = GameObject.Find("ENEMYSIDE").transform;

        selectionCamera = CameraManager.instance.selectCamera;
    }

    private void InitializeStats()
    {
        maxHp = constitution * level + 1;
        attack = (int)(strength / 5f * level + 1);
        defense = (int)(constitution / 5f * level + 1);
        speed = (int)(dexterity / 5f * level + 1);
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
                attack = (int)(attack*mod);
                break;
            case Stats.DEF:
                defense = (int)(defense * mod);
                break;
            case Stats.SPEED:
                speed = (int)(speed * mod);
                break;
            case Stats.LUCK:
                luck = (int)(luck * mod);
                break;
            default:
                break;
        }
    }

    public void Heal(float healAmount)
    {
        currentHp = (int)(currentHp + healAmount);
    }
    public int GetAttackStat()
    {
        return attack;
    }

    public int GetDefenseStat()
    {
        return defense;
    }
    public int GetSpeedStat()
    {
        return speed;
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
    }
}
