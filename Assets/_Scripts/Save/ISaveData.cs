using UnityEngine;

public interface ISaveData
{
    void SaveData(ref GameSaveData data);
    void LoadData(GameSaveData data);
}
