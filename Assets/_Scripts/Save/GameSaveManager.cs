using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Overlays;

public class GameSaveManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;

    // Data
    public static GameSaveManager instance;
    [SerializeReference] private GameSaveData saveData;

    [SerializeReference] public List<UnitSaveData> startTeamData;
    public List<UnitData> teamReferences;
    public static List<UnitSaveData> startTeam;

    private List<ISaveData> saveList;

    private FileDataHandler fileDataHandler;

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(instance); }
        else { Destroy(gameObject); }

        fileDataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        saveList = FindAllSaveObjects();
    }

    private void Start()
    {
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

    private void SaveGame()
    {
        // Pass data to other scripts
        foreach (ISaveData saveDataObj in saveList)
        {
            saveDataObj.SaveData(ref saveData);
        }

        // Save data to file
        fileDataHandler.Save(saveData);
    }

    private void LoadGame()
    {
        // Read data from file
        saveData = fileDataHandler.Load();

        if (saveData == null)
        {
            Debug.Log("No data found. Initializing new game data.");
            NewGame();
        }

        // Pass loaded data to other scripts
        foreach (ISaveData saveDataObj in saveList)
        {
            saveDataObj.LoadData(saveData);
        }
    }

    private void NewGame()
    {
        saveData = new GameSaveData();
    }

    public UnitData GetUnitReference(string name)
    {
        foreach (UnitData unit in teamReferences)
        {
            if (unit.name == name)
            {
                return unit;
            }
        }
        return null;
    }
}
