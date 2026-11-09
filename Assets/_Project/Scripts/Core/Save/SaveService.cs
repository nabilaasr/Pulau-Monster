using System.IO;
using UnityEngine;

public class SaveService
{
    public const int CurrentVersion = 1;
    private static readonly string SaveFileName = "save_0.json";

    public SaveData PendingLoad { get; set; }

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public bool HasSave() => File.Exists(SaveFilePath);

    public bool Save(SaveData data)
    {
        try
        {
            data.savedAt = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SaveFilePath, json);
            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    public SaveData Load()
    {
        try
        {
            if (!HasSave()) return null;
            string json = File.ReadAllText(SaveFilePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    public void DeleteSave()
    {
        if (HasSave()) File.Delete(SaveFilePath);
    }
}
