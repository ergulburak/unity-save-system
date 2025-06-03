using UnityEngine;

namespace ergulburak.SaveSystem
{
    [System.Serializable]
    public class SlotData : ISaveable
    {
        [SerializeField] private int currentSlotId = 1;

        public int CurrentSlotId
        {
            get => currentSlotId;
            set => currentSlotId = value;
        }
    }
}