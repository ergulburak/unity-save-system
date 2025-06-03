using UnityEngine;

namespace ergulburak.SaveSystem
{
    [CreateAssetMenu(fileName = "SaveSettings", menuName = "SaveSystem/Settings", order = 0)]
    public class SaveSettings : ScriptableObject
    {
        public string savePath = "Saves";
        public string fileExtension = ".sav";
        public bool showDebugLogs = true;
    }
}