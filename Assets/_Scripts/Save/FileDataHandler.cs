using UnityEngine;
using System;
using System.IO;

public class FileDataHandler
{
    private string dataDirPath = "";
    private string dataFileName = "";

    public FileDataHandler(string dataDirPath, string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }

    public GameSaveData Load()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        GameSaveData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // Deserialize
                loadedData = JsonUtility.FromJson<GameSaveData>(dataToLoad);
            }
            catch (Exception e) 
            {
                Debug.LogError("Failed trying to load save data file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    public void Save(GameSaveData data)
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // Serialize JSON
            string dataToStore = JsonUtility.ToJson(data, true);

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writter = new StreamWriter(stream))
                {
                    writter.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed trying to write save data file: " + fullPath + "\n" + e);
        }
    }
}
