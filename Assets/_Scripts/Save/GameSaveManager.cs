using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameSaveManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;

    // Data
    public static GameSaveManager instance;
    [SerializeReference] public GameSaveData saveData;

    [SerializeReference] public List<UnitSaveData> startTeamData;
    public List<UnitData> teamReferences;
    public static List<UnitSaveData> startTeam;

    private List<ISaveData> saveList;

    private FileDataHandler fileDataHandler;

    public string lastSceneName;
    public bool newGame;

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(instance); }
        else { Destroy(gameObject); }

        fileDataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        saveList = FindAllSaveObjects();
    }

    private void Start()
    {
        startTeam = new List<UnitSaveData>();
        startTeamData = new List<UnitSaveData>();
        foreach (UnitData unit in TeamManager.instance.teamData)
        {
            Debug.Log(unit.name);
            startTeamData.Add(unit.LoadData());
        }

        startTeam = startTeamData;
        LoadGame();
    }

    private List<ISaveData> FindAllSaveObjects()
    {
        IEnumerable<ISaveData> saveList = FindObjectsByType<MonoBehaviour>(sortMode: FindObjectsSortMode.None).OfType<ISaveData>();
        return new List<ISaveData>(saveList);
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    public void SaveGame()
    {
        // Pass data to other scripts
        foreach (ISaveData saveDataObj in saveList)
        {
            saveDataObj.SaveData(ref saveData);
        }
        string lastSceneName = SceneManager.GetActiveScene().name;

        if (lastSceneName == "Daycare")
        {
            saveData.sceneName = lastSceneName;
        } else
        {
            saveData.sceneName = "MapGeneration";
        }

        // Save data to file
        fileDataHandler.Save(saveData);
    }

    public void LoadGame()
    {
        // Read data from file
        saveData = fileDataHandler.Load();

        if (saveData == null)
        {
            Debug.Log("No data found. Initializing new game data.");
            NewGame();
        }
        lastSceneName = saveData.sceneName;
        // Pass loaded data to other scripts
        foreach (ISaveData saveDataObj in saveList)
        {
            saveDataObj.LoadData(saveData);
        }
    }

    public void NewGame()
    {
        SetStartTeam();
        saveData = new GameSaveData();
        newGame = true;

        fileDataHandler.Save(saveData);
    }

    public void NewMap()
    {
        saveData = new GameSaveData();

        fileDataHandler.Save(saveData);
    }

    public void SetStartTeam()
    {
        startTeam = new();
        startTeamData = new();
        TeamManager.instance.teamData = TeamManager.instance.startTeamData;
        foreach (UnitData unit in TeamManager.instance.teamData)
        {
            Debug.Log(unit.name);
            startTeamData.Add(unit.LoadData());
        }
        startTeam = startTeamData;
    }
}
