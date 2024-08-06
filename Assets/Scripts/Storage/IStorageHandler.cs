namespace Storage
{
    public interface IStorageHandler
    {
        bool HasKey(string key);
        void DeleteDataByKey(string key);
        void Save(string key, string value);
        string Load(string key);
    }
}