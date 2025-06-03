using static ergulburak.SaveSystem.SaveManager;
using System.Collections;
using System;

namespace ergulburak.SaveSystem
{
    public static class SaveHelper
    {
        public static bool Initialized;
        public static event Action<int> OnInitializeComplete;

        public static void Initialize()
        {
            Instance.StartCoroutine(InitializeAsync());
        }

        private static IEnumerator InitializeAsync()
        {
            SaveSystem.Initialize();
            yield return SaveSystem.InitializeAndLoadAsync(Instance.GetCurrentSlotId());

            var slotData = SaveSystem.LoadFromCache<SlotData>(Instance.GetCurrentSlotId());
            if (slotData != null)
            {
                Instance.currentSlotId = slotData.CurrentSlotId;
                $"Loaded currentSlotId: {Instance.currentSlotId}".Debug();
            }

            Initialized = true;
            Enqueue(() => { OnInitializeComplete?.Invoke(Instance.currentSlotId); });
        }

        public static T GetData<T>() where T : class, ISaveable
        {
            return SaveSystem.LoadFromCache<T>(Instance.GetCurrentSlotId());
        }

        public static void SaveData(this ISaveable saveable, Action onCompleteCallback = null)
        {
            Instance.StartCoroutine(SaveDataAsync(saveable, onCompleteCallback));
        }

        private static IEnumerator SaveDataAsync(ISaveable saveable, Action onCompleteCallback = null)
        {
            yield return SaveSystem.SaveAsync(saveable, Instance.GetCurrentSlotId());
            Enqueue(() => { onCompleteCallback?.Invoke(); });
        }

        public static void SaveGame(Action onCompleteCallback = null)
        {
            Instance.StartCoroutine(SaveGameAsync(onCompleteCallback));
        }

        private static IEnumerator SaveGameAsync(Action onCompleteCallback = null)
        {
            yield return SaveSystem.SaveAllAsync(Instance.GetCurrentSlotId());
            Enqueue(() => { onCompleteCallback?.Invoke(); });
        }

        public static void ChangeSaveSlot(int targetSlot, Action onCompleteCallback = null)
        {
            Instance.StartCoroutine(ChangeSaveSlotAsync(targetSlot, onCompleteCallback));
        }

        private static IEnumerator ChangeSaveSlotAsync(int targetSlot, Action onCompleteCallback = null)
        {
            yield return Instance.SetCurrentSlotId(targetSlot);
            Enqueue(() => { onCompleteCallback?.Invoke(); });
        }
    }
}