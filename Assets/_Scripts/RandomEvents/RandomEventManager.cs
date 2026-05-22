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
        eventIcon.sprite = currentEvent.defaultIcon;

        if (currentEvent.checkGoldToConfirm && PlayerData.Instance.gold - currentEvent.goldToGive < 0)
        {
            confirmButton.interactable = false;
        }
        else
        {
            confirmButton.interactable = true;
        }
    }

    public void EndRandomEvent()
    {
        eventUI.gameObject.SetActive(false);
    }

    public void OnConfirmEvent()
    {
        eventText.text = currentEvent.confirmText;
        eventIcon.sprite = currentEvent.confirmIcon ? currentEvent.confirmIcon : currentEvent.defaultIcon;

        for (int i = 0; i < currentEvent.eventConfirmEffect.Length; i++)
        {
            switch (currentEvent.eventConfirmEffect[i])
            {
                case Events.NONE:
                    eventText.text = $"Nothing happens...";
                    break;
                case Events.GAINGOLD:
                    PlayerData.Instance.gold += currentEvent.goldToGive;
                    break;
                case Events.GAINITEM:
                    if (currentEvent.giveRandomItem)
                    {
                        Item itemToAdd = currentEvent.itemsToGive[Random.Range(0, currentEvent.itemsToGive.Length)];
                        PlayerData.items.Add(itemToAdd);
                        eventIcon.sprite = itemToAdd.icon.GetComponent<Image>().sprite;
                    }
                    else
                    {
                        PlayerData.items.AddRange(currentEvent.itemsToGive);
                    }
                    break;
                case Events.LOSEGOLD:
                    PlayerData.Instance.gold -= currentEvent.goldToGive;
                    break;
                case Events.LOSEITEM:
                    break;
                case Events.HEAL:
                    TeamManager.instance.HealTeam(currentEvent.healingPercent);
                    break;
                case Events.LEVELUP:
                    break;
                case Events.DAMAGE:
                    TeamManager.instance.DamageTeam(currentEvent.healingPercent);
                    break;
                case Events.APPLYSTATUS:
                    TeamManager.instance.ApplyTeamStatus(currentEvent.statusToApply);
                    break;
                default:
                    break;
            }
        }

        HideButtons();
        StartCoroutine(WaitForClick());
    }

    public void OnCancelEvent()
    {
        eventText.text = currentEvent.cancelText;
        eventIcon.sprite = currentEvent.cancelIcon ? currentEvent.cancelIcon : currentEvent.defaultIcon;

        for (int i = 0; i < currentEvent.eventCancelEffect.Length; i++)
        {
            switch (currentEvent.eventCancelEffect[i])
            {
                case Events.NONE:
                    eventText.text = $"Nothing happens...";
                    break;
                case Events.GAINGOLD:
                    PlayerData.Instance.gold += currentEvent.goldToGive;
                    break;
                case Events.GAINITEM:
                    if (currentEvent.giveRandomItem)
                    {
                        PlayerData.items.Add(currentEvent.itemsToGive[Random.Range(0, currentEvent.itemsToGive.Length)]);
                    }
                    else
                    {
                        PlayerData.items.AddRange(currentEvent.itemsToGive);
                    }
                    break;
                case Events.LOSEGOLD:
                    PlayerData.Instance.gold -= currentEvent.goldToGive;
                    break;
                case Events.LOSEITEM:
                    break;
                case Events.HEAL:
                    TeamManager.instance.HealTeam(currentEvent.healingPercent);
                    break;
                case Events.LEVELUP:
                    break;
                case Events.DAMAGE:
                    TeamManager.instance.DamageTeam(currentEvent.healingPercent);
                    break;
                case Events.APPLYSTATUS:
                    TeamManager.instance.ApplyTeamStatus(currentEvent.statusToApply);
                    break;
                default:
                    break;
            }
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
            if (Input.anyKeyDown)
            {
                EndRandomEvent();
                yield break;
            }
        }
    }
}
