using System.IO;
using UnityEngine;

namespace Riftwake
{
    public static class SaveSystem
    {
        const string FileName = "riftwake_save.json";

        public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        public static bool HasSave()
        {
            return File.Exists(SavePath);
        }

        public static void Write(SaveData data)
        {
            data.HasSave = true;
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }

        public static SaveData Read()
        {
            if (!File.Exists(SavePath))
                return null;
            try
            {
                var json = File.ReadAllText(SavePath);
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch
            {
                return null;
            }
        }

        public static void Delete()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }
    }
}
