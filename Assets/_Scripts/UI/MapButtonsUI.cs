using UnityEngine;

public class MapButtonsUI : MonoBehaviour
{
    public void Pause()
    {
        PauseMenuUI.instance.ShowPauseMenu(true);
    }
}
