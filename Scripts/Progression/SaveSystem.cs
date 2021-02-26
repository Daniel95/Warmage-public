using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    private const string SAVE_PATH = "/Player.profile";

    public static bool SaveExists()
    {
        string path = Application.persistentDataPath + SAVE_PATH;

        if (!File.Exists(path))
        {
            return false;
        }

        FileStream stream = new FileStream(path, FileMode.Open);

        bool exists = stream.Length != 0;

        stream.Close();

        return exists;
    }

    public static void Save(PlayerProfile playerProfile)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + SAVE_PATH;
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, playerProfile);
        stream.Close();
    }

    public static PlayerProfile Load()
    {
        string path = Application.persistentDataPath + SAVE_PATH;

        Debug.Assert(File.Exists(path), "Save file not found in " + path);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Open);

        stream.Position = 0;

        PlayerProfile data = (PlayerProfile)formatter.Deserialize(stream);

        stream.Close();

        return data;
    }
}
