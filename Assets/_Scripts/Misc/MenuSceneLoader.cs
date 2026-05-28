using System.Collections;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneLoader : MonoBehaviour
{
    public GameObject continueButton;

    private void Start()
    {
        if (GameSaveManager.instance.newGame)
        {
            continueButton.SetActive(false);
        }
    }

    public void LoadDaycare()
    {
        SceneManager.LoadSceneAsync("Daycare");
    }

    public void NewGame()
    {
        GameSaveManager.instance.NewGame();

        StartCoroutine(Wait("MapGeneration"));
    }

    public void LoadGame()
    {
        GameSaveManager.instance.LoadGame();

        StartCoroutine(Wait(GameSaveManager.instance.lastSceneName));
       //SceneManager.LoadSceneAsync(GameSaveManager.instance.lastSceneName);
    }

    public void Exit()
    {
        Application.Quit();
    }

    private IEnumerator Wait(string sceneName)
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadSceneAsync("MapGeneration");
    }
}
