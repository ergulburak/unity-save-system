using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ergulburak.SaveSystem.Editor
{
    public class SaveSettingsWindow : EditorWindow
    {
        private SaveSettings saveSettings;
        private Type[] saveableTypes;
        private string[] saveableTypeNames;
        private int selectedTypeIndex;
        private int slotIndex;

        private void LoadISaveableTypes()
        {
            saveableTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(ISaveable).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                .OrderBy(t => t.Name)
                .ToArray();

            saveableTypeNames = saveableTypes.Select(t => t.Name).ToArray();
        }

        [MenuItem("Tools/Save Settings")]
        public static void OpenWindow()
        {
            var window = GetWindow<SaveSettingsWindow>("Save Settings");
            window.minSize = new Vector2(300, 400);
            window.maxSize = new Vector2(300, 400);
        }

        private void OnEnable()
        {
            SaveEditorHelper.CheckSaveSystem(out saveSettings);
            LoadISaveableTypes();
        }

        private void OnGUI()
        {
            if (!saveSettings)
            {
                EditorGUILayout.HelpBox("SaveSettings not found.", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("Save System Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");
            saveSettings.savePath = EditorGUILayout.TextField("Save Path", saveSettings.savePath);
            saveSettings.fileExtension = EditorGUILayout.TextField("File Extension", saveSettings.fileExtension);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(saveSettings);
                AssetDatabase.SaveAssets();
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField("Debug / Maintenance", EditorStyles.boldLabel);
            saveSettings.showDebugLogs = EditorGUILayout.Toggle("Show Debug Logs", saveSettings.showDebugLogs);

            if (GUILayout.Button("Delete All Saves", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog(
                        "Delete All Saves",
                        "Are you sure you want to delete all save files?\nThis action cannot be undone.",
                        "Yes, Delete", "Cancel"))
                {
                    DeleteAllSaves();
                }
            }

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Delete Save Data by Type", EditorStyles.boldLabel);

            if (saveableTypeNames.Length == 0)
            {
                EditorGUILayout.HelpBox("No ISaveable classes found in the project.", MessageType.Warning);
            }
            else
            {
                selectedTypeIndex = EditorGUILayout.Popup("Select Type", selectedTypeIndex, saveableTypeNames);

                if (GUILayout.Button("Delete Save File", GUILayout.Height(30)))
                {
                    Type selectedType = saveableTypes[selectedTypeIndex];

                    if (EditorUtility.DisplayDialog("Confirm Delete",
                            $"Are you sure you want to delete save data for:\n\n{selectedType.FullName}?\n\nThis cannot be undone.",
                            "Yes, Delete", "Cancel"))
                    {
                        SaveEditorHelper.DeleteSave(selectedType);
                    }
                }
            }

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Delete Save Data by Slot", EditorStyles.boldLabel);

            if (saveableTypeNames.Length > 0)
            {
                selectedTypeIndex = EditorGUILayout.Popup("Select Type", selectedTypeIndex, saveableTypeNames);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Slot Index", GUILayout.Width(80));
                int.TryParse(EditorGUILayout.TextField(slotIndex.ToString(), GUILayout.Width(50)), out slotIndex);
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Delete Selected Slot Save", GUILayout.Height(30)))
                {
                    Type selectedType = saveableTypes[selectedTypeIndex];

                    if (EditorUtility.DisplayDialog("Confirm Delete",
                            $"Are you sure you want to delete slot {slotIndex} for:\n\n{selectedType.FullName}?\n\nThis cannot be undone.",
                            "Yes, Delete", "Cancel"))
                    {
                        SaveEditorHelper.DeleteSave(selectedType, slotIndex);
                    }
                }
            }
        }

        private void DeleteAllSaves()
        {
            string fullSavePath = Path.Combine(Application.persistentDataPath, saveSettings.savePath);

            if (Directory.Exists(fullSavePath))
            {
                string[] files = Directory.GetFiles(fullSavePath, "*" + saveSettings.fileExtension);
                int deleteCount = 0;

                foreach (var file in files)
                {
                    File.Delete(file);
                    deleteCount++;
                }

                SaveEditorDebugHelper.Debug(
                    $"[{nameof(SaveSettingsWindow)}] {deleteCount} save file deleted. ({fullSavePath})");
                AssetDatabase.Refresh();
            }
            else
            {
                SaveEditorDebugHelper.DebugWarning($"Save file not found: {fullSavePath}");
            }
        }
    }
}