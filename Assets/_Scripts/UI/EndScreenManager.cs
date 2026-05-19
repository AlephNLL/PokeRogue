using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class EndScreenManager : MonoBehaviour
{
    public static EndScreenManager instance;
    [SerializeField] Canvas confirmScreen;
    [SerializeField] Canvas endScreen;
    [SerializeField] GameObject monSelectionButtonPrefab;
    [SerializeField] GameObject monLevelUpWindowPrefab;
    [SerializeField] TextMeshProUGUI goldText;
    [SerializeField] TextMeshProUGUI expText;
    [SerializeField] TextMeshProUGUI selectMonText;
    GameObject monToCapture;
    static bool dontShowConfirmScreen = false;
    public static bool monSelected = false;
    List<GameObject> monButtons;

    private void Awake()
    {
        instance = this;
        monSelected = false;
        monButtons = new List<GameObject>();
    }
    public void ShowVictoryScreen(Unit[]playerTeam, GameObject[] enemyMons, int gold, int exp)
    {
        endScreen.gameObject.SetActive(true);

        goldText.text = gold.ToString();
        expText.text = exp.ToString();

        for (int i = 0; i < enemyMons.Length; i++)
        {
            GameObject monButton = Instantiate(monSelectionButtonPrefab, endScreen.transform);
            monButton.GetComponent<RectTransform>().localPosition = new Vector3(250 * (i - (enemyMons.Length - 1) / 2f), -250, 0);
            monButton.GetComponentInChildren<TextMeshProUGUI>().text = enemyMons[i].name;
            monToCapture = enemyMons[i];
            monButton.GetComponent<Button>().onClick.AddListener(delegate { TeamManager.instance.AddNewTeamMember(monToCapture); });
            monButtons.Add(monButton);
            monButton.GetComponent<Image>().sprite = monToCapture.GetComponent<Unit>().icon;
        }

        for (int i = 0; i < playerTeam.Length; i++)
        {
            GameObject monLevelUpScreen = Instantiate(monLevelUpWindowPrefab, endScreen.transform);
            monLevelUpScreen.GetComponent<RectTransform>().localPosition = new Vector3(-272 + (i % 2) * 500, 140 + -(i / 2) * 150, 0.0f);
            monLevelUpScreen.GetComponentInChildren<TMP_Text>().text = playerTeam[i].name;
            monLevelUpScreen.transform.GetChild(0).GetComponent<Image>().sprite = playerTeam[i].icon;

            StartCoroutine(ManageEXPGain(PlayerData.teamData[i], monLevelUpScreen.transform.GetChild(2).GetComponent<Slider>(),
                playerTeam[i].expCurve, exp, monLevelUpScreen.transform.GetChild(3).gameObject));
        }
    }
    public void ShowVictoryScreen(Unit[] playerTeam,GameObject enemyMon, int gold, int exp)
    {
        GameObject[] mons = new GameObject[1];
        mons[0] = enemyMon;
        ShowVictoryScreen(playerTeam,mons, gold, exp);
    }
    IEnumerator ManageEXPGain(UnitData monData, Slider expBar, ExpCurve expCurve, int expGiven, GameObject levelUpNotif)
    {
        int currentLevel = monData.level;
        int currentExp = monData.currentExp;
        int expToProcess = expGiven;
        int levelsGained = 0;

        while (expToProcess > 0)
        {
            int expNeededForNextLevel = expCurve.expPerLevel[currentLevel - 1];
            int expMissingForNextLevel = expNeededForNextLevel - currentExp;

            if (expToProcess >= expMissingForNextLevel)
            {
                float startVal = (float)currentExp / expNeededForNextLevel;
                yield return UpdateExpBar(expBar, startVal, 1f);

                expToProcess -= expMissingForNextLevel;
                currentLevel++;
                levelsGained++;
                currentExp = 0;

                monData.level = currentLevel;

                TeamManager.instance.HealMon(monData, 1);
                MoveLearner.instance.LearnMove(monData, currentLevel);
            }
            else
            {
                float startVal = (float)currentExp / expNeededForNextLevel;
                currentExp += expToProcess;
                float endVal = (float)currentExp / expNeededForNextLevel;

                yield return UpdateExpBar(expBar, startVal, endVal);

                expToProcess = 0;
            }
        }

        if (levelsGained > 0)
        {
            levelUpNotif.gameObject.SetActive(true);
        }

        monData.level = currentLevel;
        monData.currentExp = currentExp;
    }
    IEnumerator UpdateExpBar(Slider expBar, float startValue, float endValue)
    {
        float t = 0;

        while (t<1) 
        {
            expBar.value = Mathf.Lerp(startValue,endValue,t);
            t += Time.deltaTime;
            yield return null;
        }

        expBar.value = endValue;
    }
    public void Continue()
    {
        if (monSelected || dontShowConfirmScreen) BackToMapScreen();
        else
        { 
            confirmScreen.gameObject.SetActive(true);
            endScreen.gameObject.SetActive(false);
        }
    }

    public void BackToMapScreen()
    {
        AudioManager.instance.StopMusic();
        MapManager.instance.LoadMapScene();
    }

    public void ReturnToVictoryScreen()
    {
        confirmScreen.gameObject.SetActive(false);
        endScreen.gameObject.SetActive(true);
    }

    public void SetDontShowConfirm()
    {
        dontShowConfirmScreen = true;
    }

    public void EndMonSelection(Unit monSelected, bool sentToDayCare = false)
    {
        for (int i = 0; i < monButtons.Count; i++)
        {
            monButtons[i].GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(monButtons[i]);
        }
        selectMonText.text = $"You captured a {monSelected.name}";

        if (sentToDayCare) 
        {
            selectMonText.text = $"Your party is full, {monSelected.name} was sent to the Daycare.";
        }
        
        monButtons.Clear();
    }
}
