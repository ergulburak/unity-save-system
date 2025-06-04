using System.Runtime.CompilerServices;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Debug(string text)
        {
            if (!Settings) UnityEngine.Debug.Log(text);
            else if (Settings.showDebugLogs) UnityEngine.Debug.Log(text);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DebugWarning(string text)
        {
            if (!Settings) UnityEngine.Debug.LogWarning(text);
            else if (Settings.showDebugLogs) UnityEngine.Debug.LogWarning(text);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DebugError(string text)
        {
            if (!Settings) UnityEngine.Debug.LogError(text);
            else if (Settings.showDebugLogs) UnityEngine.Debug.LogError(text);
        }
    }
}