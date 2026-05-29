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

    public string FilePath()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        return fullPath;
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

    public void DeleteSave(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("deleted file at: " + path);
        }
        else
        {
            Debug.Log("no file foudn at: " + path);
        }
    }

    public bool DoesSaveFileExist()
    {
        string path = Path.Combine(dataDirPath, dataFileName);

        bool exists = File.Exists(path);    
        Debug.Log(exists);
        return exists;
    }

}
