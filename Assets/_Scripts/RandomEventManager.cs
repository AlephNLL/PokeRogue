using GameData;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomEventManager : MonoBehaviour
{
    public static RandomEventManager instance;
    [SerializeField] RandomEvent[] eventPool;
    RandomEvent currentEvent;

    [SerializeField] GameObject eventUI;
    [SerializeField] TMP_Text eventText;
    [SerializeField] Image eventIcon;

    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;

    private void Awake()
    {
        instance = this;
    }
    public void CreateRandomEvent()
    {
        currentEvent = eventPool[Random.Range(0,eventPool.Length)];

        eventUI.gameObject.SetActive(true);
        ShowButtons();
        eventText.text = currentEvent.eventText;
        eventIcon.sprite = currentEvent.icon;
    }

    public void EndRandomEvent()
    {
        eventUI.gameObject.SetActive(false);
    }

    public void OnConfirmEvent()
    {
        eventText.text = currentEvent.confirmText;
        switch (currentEvent.eventConfirmEffect)
        {
            case Events.NONE:
                eventText.text = $"Nothing happens...";
                break;
            case Events.GAINGOLD:
                PlayerData.Instance.gold += currentEvent.goldToGive;
                break;
            case Events.GAINITEM:
                PlayerData.items.Add(currentEvent.itemToGive);
                break;
            case Events.LOSEGOLD:
                PlayerData.Instance.gold -= currentEvent.goldToGive;
                break;
            case Events.LOSEITEM:
                break;
            case Events.HEAL:
                TeamManager.instance.HealTeam(1);
                break;
            case Events.LEVELUP:
                break;
            case Events.DAMAGE:
                TeamManager.instance.DamageTeam(.2f);
                break;
            case Events.APPLYSTATUS:
                TeamManager.instance.ApplyTeamStatus(currentEvent.statusToApply);
                break;
            default:
                break;
        }

        HideButtons();
        StartCoroutine(WaitForClick());
    }

    public void OnCancelEvent()
    {
        eventText.text = currentEvent.cancelText;

        switch (currentEvent.eventCancelEffect)
        {
            case Events.NONE:
                eventText.text = $"Nothing happens...";
                break;
            case Events.GAINGOLD:
                PlayerData.Instance.gold += currentEvent.goldToGive;
                break;
            case Events.GAINITEM:
                PlayerData.items.Add(currentEvent.itemToGive);
                break;
            case Events.LOSEGOLD:
                PlayerData.Instance.gold -= currentEvent.goldToGive;
                break;
            case Events.LOSEITEM:
                break;
            case Events.HEAL:
                TeamManager.instance.HealTeam(1);
                break;
            case Events.LEVELUP:
                break;
            case Events.DAMAGE:
                TeamManager.instance.DamageTeam(.2f);
                break;
            case Events.APPLYSTATUS:
                TeamManager.instance.ApplyTeamStatus(currentEvent.statusToApply);
                break;
            default:
                break;
        }

        HideButtons();
        StartCoroutine(WaitForClick());
    }

    void HideButtons()
    {
        confirmButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);
    }

    void ShowButtons()
    {
        confirmButton.gameObject.SetActive(true);
        cancelButton.gameObject.SetActive(true);
    }

    IEnumerator WaitForClick()
    {
        while (true)
        {
            yield return null;
            if (Input.GetMouseButtonDown(0))
            {
                EndRandomEvent();
                yield break;
            }
        }
    }
}
