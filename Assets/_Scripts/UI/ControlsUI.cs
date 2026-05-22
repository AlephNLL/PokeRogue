using UnityEngine;

public class ControlsUI : MonoBehaviour
{
    public static ControlsUI instance;
    public GameObject[] controlTabs;

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
    public void ShowSelectionControls(bool mouse = false)
    {
        if(!mouse) controlTabs[0].SetActive(true);
        else controlTabs[1].SetActive(true);
    }

    public void HideSelectionControls(bool mouse = false)
    {
        controlTabs[0].SetActive(false);
        controlTabs[1].SetActive(false);
    }

    public void ShowSummaryControls()
    {
        controlTabs[2].SetActive(true);
    }

    public void HideSummaryControls()
    {
        controlTabs[2].SetActive(false);
    }

    public void ShowConfirm()
    {
        controlTabs[3].SetActive(true);
    }

    public void HideConfirm()
    {
        controlTabs[3].SetActive(false);
    }
}
