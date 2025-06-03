using UnityEngine;

namespace ergulburak.SaveSystem.Editor
{
    public static class SaveEditorDebugHelper
    {
        private static SaveSettings _settings;

        private static SaveSettings Settings
        {
            get
            {
                if (!_settings)
                    _settings = Resources.Load<SaveSettings>("SaveSystem/SaveSettings");
                return _settings;
            }
        }

        public static void Debug(string text)
        {
            if (Settings.showDebugLogs) UnityEngine.Debug.Log(text);
        }

        public static void DebugWarning(string text)
        {
            if (Settings.showDebugLogs) UnityEngine.Debug.LogWarning(text);
        }

        public static void DebugError(string text)
        {
            if (Settings.showDebugLogs) UnityEngine.Debug.LogError(text);
        }
    }
}