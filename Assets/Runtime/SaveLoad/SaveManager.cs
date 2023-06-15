using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Runtime.SaveLoad {
    public class SaveManager {
        private static readonly string SaveFolder = Application.persistentDataPath + "/GameData";

        public static void Save<T>(SaveProfile<T> saveProfile) where T : SaveData {
            var filePath = $"{SaveFolder}/{saveProfile.profileName}";
            if (!Directory.Exists(SaveFolder)) Directory.CreateDirectory(SaveFolder);
            var jsonString = JsonConvert.SerializeObject(saveProfile, Formatting.Indented,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            File.WriteAllText(filePath, jsonString);
            Debug.Log($"Successfully save to {filePath}");
        }

        public static SaveProfile<T> Load<T>(string profileName) where T : SaveData {
            var filePath = $"{SaveFolder}/{profileName}";
            if (!File.Exists(filePath)) {
                throw new SaveLoadException($"Save data {profileName} does not exist.");
            }

            var jsonString = File.ReadAllText(filePath);
            Debug.Log($"Successfully load from {filePath}");
            return JsonConvert.DeserializeObject<SaveProfile<T>>(jsonString);
        }

        public static void Deleted(string profileName) {
            var filePath = $"{SaveFolder}/{profileName}";
            if (!File.Exists(filePath)) {
                throw new SaveLoadException($"Save data {profileName} does not exist.");
            }

            File.Delete(filePath);
            Debug.Log($"Successfully delete {filePath}");
        }


        private class SaveLoadException : Exception {
            public SaveLoadException(string message) : base(message) { }
        }
    }
}