using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DaycareUIManager : MonoBehaviour
{
    public static DaycareUIManager instance;

    public Button fusionButton;
    public Button battleButton;

    public Button battleConmfirmButton;

    public GameObject tooltipBorder;
    public TMP_Text tooltipText;

    public GameObject confirmScreen;
    private void Awake()
    {
        instance = this;
    }
    public void ShowTooltipText(string text)
    {
        tooltipBorder.SetActive(true);
        tooltipText.text = text;
    }

    public void HideTooltipText()
    {
        tooltipBorder.SetActive(false);
    }

    public void ShowConfirmScreen()
    {
        confirmScreen.SetActive(true);
        HideMainButtons();
    }
    public void HideConfirmScreen()
    {
        confirmScreen.SetActive(false);
    }
    public void DisableAllButtons()
    {
        fusionButton.interactable = false;
        battleButton.interactable = false;
    }

    public void ShowMainButtons()
    {
        fusionButton.gameObject.SetActive(true);
        battleButton.gameObject.SetActive(true);

        fusionButton.interactable = true;
        battleButton.interactable = true;
    }
    public void HideMainButtons()
    {
        fusionButton.gameObject.SetActive(false);
        battleButton.gameObject.SetActive(false);
    }

    public void ShowBattleConfirm()
    {
        battleConmfirmButton.gameObject.SetActive(true);
    }
    public void HideBattleConfirm()
    {
        battleConmfirmButton.gameObject.SetActive(false);
    }
    public void EnableBattleConfirm()
    {
        battleConmfirmButton.enabled = true;
    }
    public void DisableBattleConfirm()
    {
        battleConmfirmButton.enabled = false;
    }
}
