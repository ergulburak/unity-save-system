using System.Runtime.CompilerServices;
using UnityEngine;

namespace ergulburak.SaveSystem
{
    public static class SaveDebugHelper
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
        public static void Debug(this string text)
        {
            if (Settings.showDebugLogs) UnityEngine.Debug.Log(text);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DebugWarning(this string text)
        {
            if (Settings.showDebugLogs) UnityEngine.Debug.LogWarning(text);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DebugError(this string text)
        {
            if (Settings.showDebugLogs) UnityEngine.Debug.LogError(text);
        }
    }
}