using GameData;
using UnityEngine;
using UnityEngine.SceneManagement;

public class test : MonoBehaviour
{
    public Difficulty battleDifficulty;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            BattleGenerator.Instance.GenerateTeam(battleDifficulty);
            SceneManager.LoadSceneAsync("BattleScene");
        }
    }
}
