using GameData;
using UnityEngine;
using UnityEngine.SceneManagement;

public class test : MonoBehaviour
{
    public Difficulty battleDifficulty;

    private void Start()
    {
        BattleData.Difficulty = battleDifficulty;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            RandomEventManager.instance.CreateRandomEvent();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (PlayerData.daycareTeamData == null) PlayerData.daycareTeamData = PlayerData.teamData;
            else { PlayerData.daycareTeamData.AddRange(PlayerData.teamData); }
            MapView.instance.team.Clear();
            MapManager.instance.LoadScene("Daycare");
        }
    }
}
