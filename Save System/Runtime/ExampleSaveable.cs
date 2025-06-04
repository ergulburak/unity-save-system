namespace ergulburak.SaveSystem
{
    using UnityEngine;

    [System.Serializable]
    public class ExampleData : ISaveable
    {
        public int counter = 100;
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
                var exampleData = SaveHelper.GetData<ExampleData>();
                if (exampleData != null)
                {
                    exampleData.counter--;
                    Debug.Log($"Loaded ExampleData from cache: Counter={exampleData.counter}");
                    exampleData.SaveData();
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) SaveHelper.ChangeSaveSlot(1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SaveHelper.ChangeSaveSlot(2);
        }
    }
}