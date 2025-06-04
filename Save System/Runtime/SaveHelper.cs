using static ergulburak.SaveSystem.SaveManager;
using System.Collections.Generic;
using System.Collections;
using System;

namespace ergulburak.SaveSystem
{
    public static class SaveHelper
    {
        private static readonly Queue<(ISaveable saveable, Action callback)> _saveQueue = new();
        private static readonly Queue<(ISaveable saveable, Action callback)> _checkQueue = new();
        private static bool _isSaving = false;
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

        public static void DisposeQueue()
        {
            _saveQueue.Clear();
            _isSaving = false;
        }

        public static T GetData<T>() where T : class, ISaveable
        {
            return SaveSystem.LoadFromCache<T>(Instance.GetCurrentSlotId());
        }

        public static void SaveData(this ISaveable saveable, Action onCompleteCallback = null)
        {
            _checkQueue.Clear();
            foreach (var item in _saveQueue)
            {
                if (item.saveable.GetType() != saveable.GetType())
                    _checkQueue.Enqueue(item);
            }

            _checkQueue.Enqueue((saveable, onCompleteCallback));
            _saveQueue.Clear();
            foreach (var item in _checkQueue)
                _saveQueue.Enqueue(item);

            TryStartNextSave();
        }

        private static void TryStartNextSave()
        {
            if (_isSaving || _saveQueue.Count == 0)
                return;

            var (saveable, callback) = _saveQueue.Dequeue();
            Instance.StartCoroutine(SaveRoutine(saveable, callback));
        }

        private static IEnumerator SaveRoutine(ISaveable saveable, Action callback)
        {
            _isSaving = true;

            var task = SaveSystem.SaveAsync(saveable, Instance.GetCurrentSlotId());
            while (!task.IsCompleted) yield return null;

            if (task.Exception != null)
                $"Save failed for {saveable.GetType().Name}: {task.Exception}".DebugError();
            else
                callback?.Invoke();

            _isSaving = false;

            TryStartNextSave();
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