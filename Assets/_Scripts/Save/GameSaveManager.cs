using System;
using UnityEngine;

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager instance;
    private GameSaveData SaveData;

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(instance); }
        else { Destroy(gameObject); }
    }

    private void SaveGame()
    {
        throw new NotImplementedException();
    }

    private void LoadGame()
    {
        throw new NotImplementedException();
    }

    private void NewGame()
    {
        throw new NotImplementedException();
    }
}
