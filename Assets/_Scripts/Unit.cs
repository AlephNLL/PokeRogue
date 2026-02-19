using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using GameData;
public class Unit : MonoBehaviour
{
    [Header("Properties")]
    new public string name;
    public string description;
    public Stance[] stances;
    public Stance currentStance;
    public int level;

    [Header("Base Stats")]
    public int strength;
    public int constitution;
    public int dexterity;
    public int intelligence;
    public int charisma;

    [Header("Stats")]
    [SerializeField]
    private int maxHp;
    private int currentHp;
    [SerializeField]
    private int attack;
    [SerializeField]
    private int defense;
    [SerializeField]
    private int spattack;
    [SerializeField]
    private int spdefense;
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
    private Button attackButton;
    [SerializeField]
    private Button runButton;

    private void Awake()
    {
        InitializeStats();

        actionCamera.LookAt = GameObject.Find("ENEMYSIDE").transform;
    }

    private void InitializeStats()
    {
        maxHp = constitution * level + 1;
        attack = (int)(strength / 5f * level + 1);
        defense = (int)(constitution / 5f * level + 1);
        spattack = (int)(intelligence / 5f * level + 1);
        spdefense = (int)(charisma / 5f * level + 1);
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
        selectionCamera.gameObject.SetActive(true);
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

        attackButton.onClick.AddListener(delegate { TBBS.instance.Attack(this); });
        runButton.onClick.AddListener(delegate { TBBS.instance.Run(this); });
    }
    public void CloseBattleMenu()
    {
        if (battleMenu == null) return;
        battleMenu.gameObject.SetActive(false);

        attackButton.onClick.RemoveAllListeners();
        runButton.onClick.RemoveAllListeners();
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
}
