using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            
            int index = i;
            GameObject monToCapture = enemyMons[index];

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
        float expBoost = monData.HasAbility("Overgrowth") ? 1.5f : 1f;
        int expToProcess = (int)(expGiven * expBoost);
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
        if (MapManager.instance.currentNode.roomType == GameData.RoomType.Boss)
        {
            MapManager.instance.mapCreated = false;
            MapManager.instance.createdRooms.Clear();
            MapManager.instance.nodes.Clear();
            PlayerData.daycareTeamData.AddRange(PlayerData.teamData);
            SceneManager.LoadSceneAsync("Daycare");
        }
        else
        {
            MapManager.instance.LoadMapScene();
        }
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

    public void EndMonSelection(Unit monSelected, bool sentToDayCare = false, string boxedName = "")
    {
        for (int i = 0; i < monButtons.Count; i++)
        {
            monButtons[i].GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(monButtons[i]);
        }
        monButtons.Clear();

        if (sentToDayCare && !string.IsNullOrEmpty(boxedName))
        {
            selectMonText.text = $"{boxedName} was sent to the Daycare.";
        }
        else if (sentToDayCare)
        {
            selectMonText.text = $"Your party is full, {monSelected.name} was sent to the Daycare.";
        }
        else
        {
            selectMonText.text = $"You captured a {monSelected.name}";
        }

        EndScreenManager.monSelected = true;
    }

    public void PromptDaycareSelection(UnitData newMonData, Unit newMonUnit)
    {
        for (int i = 0; i < monButtons.Count; i++)
        {
            Destroy(monButtons[i]);
        }
        monButtons.Clear();

        selectMonText.text = "Party is full! Choose who to send to the Daycare:";

        List<UnitData> allOptions = new List<UnitData>(PlayerData.teamData);
        allOptions.Add(newMonData);

        for (int i = 0; i < allOptions.Count; i++)
        {
            GameObject monButton = Instantiate(monSelectionButtonPrefab, endScreen.transform);

            monButton.GetComponent<RectTransform>().localPosition = new Vector3(200 * (i - (allOptions.Count - 1) / 2f), -250, 0);
            monButton.GetComponentInChildren<TextMeshProUGUI>().text = allOptions[i].name;

            int index = i;
            UnitData selectedData = allOptions[index];

            monButton.GetComponent<Button>().onClick.AddListener(delegate {
                ResolveDaycareSelection(selectedData, newMonData, newMonUnit);
            });

            monButtons.Add(monButton);
            monButton.GetComponent<Image>().sprite = selectedData.prefab.GetComponent<Unit>().icon;
        }
    }

    private void ResolveDaycareSelection(UnitData chosenToBox, UnitData newMonData, Unit newMonUnit)
    {
        if (PlayerData.daycareTeamData == null) PlayerData.daycareTeamData = new List<UnitData>();

        if (chosenToBox != newMonData)
        {
            PlayerData.teamData.Remove(chosenToBox);
            PlayerData.daycareTeamData.Add(chosenToBox);

            PlayerData.teamData.Add(newMonData);

            EndMonSelection(newMonUnit, true, chosenToBox.name);
        }
        else
        {
            PlayerData.daycareTeamData.Add(newMonData);

            EndMonSelection(newMonUnit, true, newMonData.name);
        }

        for (int i = 0; i < PlayerData.teamData.Count; i++)
        {
            PlayerData.teamData[i].id = i;
        }
    }
}
