using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI instance;
    public GameObject tooltipBorder;
    public TMP_Text tooltipText;
    public float textMinDuration = 2;
    public List<string> scheduledTexts = new List<string>();

    bool isTextActive;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void InstantShowText(string text)
    {
        tooltipBorder.SetActive(true);
        tooltipText.text = text;
    }
    public void ShowTooltipText(string text)
    {
        if (isTextActive)
        {
            scheduledTexts.Add(text);
            return;
        }

        InstantShowText(text);

        StartCoroutine(TextTimer());
    }
    IEnumerator TextTimer()
    {
        isTextActive = true;
        yield return new WaitForSeconds(textMinDuration);
        HideTooltipText();
        yield return new WaitForSeconds(.1f);
        if (scheduledTexts.Count > 0)
        {
            ShowTooltipText(scheduledTexts[0]);
            scheduledTexts.RemoveAt(0);
        }
    }
    public void HideTooltipText()
    {
        isTextActive = false;
        tooltipBorder.SetActive(false);
    }
}
