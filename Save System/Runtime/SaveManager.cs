using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

namespace ergulburak.SaveSystem
{
    public class SaveManager : MonoBehaviour
    {
        private static readonly ConcurrentQueue<Action> actionQueue = new();
        public static SaveManager Instance { get; private set; }

        internal int currentSlotId = 1;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SaveHelper.Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            SaveHelper.DisposeQueue();
        }

        private void Update()
        {
            while (actionQueue.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }

        public static void Enqueue(Action action)
        {
            actionQueue.Enqueue(action);
        }

        internal async Task SetCurrentSlotId(int newSlotId)
        {
            try
            {
                currentSlotId = newSlotId;
                var slotData = SaveSystem.LoadFromCache<SlotData>(currentSlotId) ?? new SlotData();

                slotData.CurrentSlotId = newSlotId;
                await SaveSystem.SaveAsync(slotData, currentSlotId);
                $"CurrentSlotId updated and saved: {newSlotId}".Debug();
            }
            catch (Exception ex)
            {
                $"Failed to set CurrentSlotId: {ex.Message}".DebugError();
            }
        }

        public int GetCurrentSlotId()
        {
            return currentSlotId;
        }
    }
}