using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ergulburak.SaveSystem.Editor
{
    public static class SaveEditorHelper
    {
        private const string saveSettingsPath = "Assets/Resources/SaveSystem/SaveSettings.asset";

        public static void CheckSaveSystem(out SaveSettings saveSettings)
        {
            saveSettings = AssetDatabase.LoadAssetAtPath<SaveSettings>(saveSettingsPath);

            if (saveSettings != null) return;

            SaveEditorDebugHelper.DebugWarning("SaveSettings not found in Resources folder. Creating a new one...");

            string resourcesPath = "Assets/Resources";
            string currencySystemPath = "Assets/Resources/SaveSystem";

            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder(currencySystemPath))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "SaveSystem");
            }

            saveSettings = ScriptableObject.CreateInstance<SaveSettings>();
            AssetDatabase.CreateAsset(saveSettings, saveSettingsPath);
            AssetDatabase.SaveAssets();
        }

        public static void DeleteSave(Type saveableType)
        {
            string keyPrefix = saveableType.FullName;
            string saveDir = SaveSystem.SaveDirectory;
            string ext = SaveSystem.FileExtension;

            if (!Directory.Exists(saveDir))
            {
                SaveEditorDebugHelper.DebugWarning($"[SaveHelper] Save directory does not exist: {saveDir}");
                return;
            }

            string[] files = Directory.GetFiles(saveDir, "*" + ext);
            var matchingFiles = files.Where(f => Path.GetFileNameWithoutExtension(f).StartsWith(keyPrefix)).ToArray();

            if (matchingFiles.Length == 0)
            {
                SaveEditorDebugHelper.DebugWarning($"[SaveHelper] No save files found for {keyPrefix}");
                return;
            }

            foreach (var file in matchingFiles)
            {
                File.Delete(file);
                SaveEditorDebugHelper.Debug($"[SaveHelper] Deleted: {file}");
            }
        }

        public static void DeleteSave(Type saveableType, int slot)
        {
            string key = $"{saveableType.FullName}_{slot}";
            string path = SaveSystem.GetSaveFilePath(key);

            if (File.Exists(path))
            {
                File.Delete(path);
                SaveEditorDebugHelper.Debug($"[SaveHelper] Deleted slot {slot} for {saveableType.FullName}: {path}");
            }
            else
            {
                SaveEditorDebugHelper.DebugWarning(
                    $"[SaveHelper] Save file for {saveableType.FullName} with slot {slot} not found.");
            }
        }
    }
}