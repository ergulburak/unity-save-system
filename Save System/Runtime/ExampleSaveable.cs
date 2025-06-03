namespace ergulburak.SaveSystem
{
    using UnityEngine;

    [System.Serializable]
    public class PlayerData : ISaveable
    {
        public int health = 100;
        public Vector3 position;
    }

    [System.Serializable]
    public class GameSettings : ISaveable
    {
        public float volume = 1f;
        public bool isFullScreen = true;
    }

    public class ExampleSaveable : MonoBehaviour
    {
        private void OnEnable()
        {
            if (!SaveHelper.Initialized)
                SaveHelper.OnInitializeComplete += OnInitializeComplete;
        }

        private void OnDisable()
        {
            SaveHelper.OnInitializeComplete -= OnInitializeComplete;
        }

        private void OnInitializeComplete(int slotId)
        {
            Debug.Log($"Save data loaded for slot {slotId}. Ready to use!");
        }

        private void Update()
        {
            if (!SaveHelper.Initialized) return;

            if (Input.GetKeyDown(KeyCode.S))
                SaveHelper.SaveGame(() => { "Saving game.".Debug(); });

            if (Input.GetKeyDown(KeyCode.L))
            {
                var playerData = SaveHelper.GetData<PlayerData>();
                if (playerData != null)
                {
                    playerData.health--;
                    Debug.Log(
                        $"Loaded PlayerData from cache: Health={playerData.health}, Position={playerData.position}");
                    playerData.SaveData();
                }

                var settings = SaveHelper.GetData<GameSettings>();
                if (settings != null)
                {
                    Debug.Log(
                        $"Loaded GameSettings from cache: Volume={settings.volume}, FullScreen={settings.isFullScreen}");
                }

                settings.SaveData();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) SaveHelper.ChangeSaveSlot(1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SaveHelper.ChangeSaveSlot(2);
        }
    }
}