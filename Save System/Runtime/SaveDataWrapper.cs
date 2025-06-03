namespace ergulburak.SaveSystem
{
    [System.Serializable]
    public class SaveDataWrapper<T> where T : ISaveable
    {
        public int version = 1;
        public T data;

        public SaveDataWrapper(T data)
        {
            this.data = data;
        }
    }
}