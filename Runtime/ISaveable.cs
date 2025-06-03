namespace ergulburak.SaveSystem
{
    public interface ISaveable
    {
        string SaveKey => GetType().FullName;
    }
}