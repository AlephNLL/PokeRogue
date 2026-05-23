using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI instance;

    public GameObject pauseCanvas;
    public GameObject optionsCanvas;
    public GameObject exitConfirmation;
    public GameObject menuButtons;
    public GameObject mainMenuButtons;

    private bool isPaused = false;

    private void Start()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(instance.gameObject); }
        else { Destroy(gameObject); }
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu") { return; }

        if (isPaused) { Time.timeScale = 0f; }
        else { Time.timeScale = 1f; }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowPauseMenu(!isPaused);
        }
    }

    public void ShowPauseMenu(bool state)
    {
        if (pauseCanvas == null) { return; }

        isPaused = state;
        pauseCanvas.SetActive(state);
    }

    public void PauseGame()
    {
        isPaused = true;
    }

    public void Resume()
    {
        ShowPauseMenu(false);
    }

    public void ExitToMenu()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    public void ShowExitConfirmation(bool state = true)
    {
        menuButtons.SetActive(false);
        optionsCanvas.SetActive(false);
        exitConfirmation.SetActive(state);
    }

    public void ShowOptions(bool state = false)
    {
        if (mainMenuButtons != null) { mainMenuButtons.SetActive(false); }

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            menuButtons.SetActive(false);
        }
        exitConfirmation.SetActive(false);
        optionsCanvas.SetActive(state);
    }

    public void ShowMenuButtons(bool state)
    {
        if(SceneManager.GetActiveScene().name == "MainMenu") 
        {
            if (mainMenuButtons == null) { return; }

            optionsCanvas.SetActive(false);
            exitConfirmation.SetActive(false);
            mainMenuButtons.SetActive(state);
        }
        else
        {
            if (menuButtons == null) { return; }

            optionsCanvas.SetActive(false);
            exitConfirmation.SetActive(false);
            menuButtons.SetActive(state);
        }

        
    }
}
