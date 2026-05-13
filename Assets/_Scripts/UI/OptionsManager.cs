using UnityEngine;

public class OptionsManager : MonoBehaviour
{
    public bool isPaused = false;
    public Canvas menu;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            { 
                PauseGame(); 
                isPaused = true; 
                ShowMenu();
            }
            else 
            { 
                ResumeGame(); 
                isPaused = false; 
                ShowMenu(false);
            }
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    private void ShowMenu(bool state = true)
    {
        if (menu == null) { return; }
        menu.enabled = state;
    }
}
