using UnityEditor;
using UnityEngine;

namespace ergulburak.SaveSystem.Editor
{
    [CustomEditor(typeof(SaveSettings))]
    public class SaveSettingsDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This asset is managed exclusively through the configuration window.",
                MessageType.Info);
            if (GUILayout.Button("Open Configuration Window"))
            {
                SaveSettingsWindow.OpenWindow();
            }
        }
    }
}