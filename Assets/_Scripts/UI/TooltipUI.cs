using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI instance;
    public GameObject tooltipBorder;
    public TMP_Text tooltipText;
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
    public void ShowTooltipText(string text)
    {
        tooltipBorder.SetActive(true);
        tooltipText.text = text;
    }

    public void HideTooltipText()
    {
        tooltipBorder.SetActive(false);
    }
}
