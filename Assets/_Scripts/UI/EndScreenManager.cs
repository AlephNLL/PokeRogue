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
        }
    }
    public void ShowVictoryScreen(Unit[] playerTeam,GameObject enemyMon, int gold, int exp)
    {
        GameObject[] mons = new GameObject[1];
        mons[0] = enemyMon;
        ShowVictoryScreen(playerTeam,mons, gold, exp);
    }

    public void Continue()
    {
        if (monSelected || dontShowConfirmScreen) MapManager.instance.LoadMapScene();
        else
        { 
            confirmScreen.gameObject.SetActive(true);
            endScreen.gameObject.SetActive(false);
        }
    }

    public void BackToMapScreen()
    {
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
