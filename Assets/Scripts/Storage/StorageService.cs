using Newtonsoft.Json;
using UnityEngine;

namespace Storage
{
    public class StorageService : MonoBehaviour
    {
        public static StorageService Instance { get; private set; }
        private readonly IStorageHandler currentHandler = new PlayerPrefsStorageHandler();

        private void Awake()
        {
            InitSingleton();
        }

        public void SaveData<T>(T data, string id = "")
        {
            var serializedData = Serialize(data);
            var dataKey = GetDataKey<T>(id);
            SaveSerializedData(dataKey, serializedData);
            Debug.Log($"[{nameof(StorageService)}] - Saving data. Key: {dataKey}. Data: {serializedData}");
        }

        public T LoadData<T>(string id = "") where T : new()
        {
            var data = new T();
            var dataKey = GetDataKey<T>(id);

            if (HasKey(dataKey))
            {
                var serializedData = LoadSerializedData(dataKey);
                data = Deserialize<T>(serializedData);
                Debug.Log($"[{nameof(StorageService)}] - Loading data. Key: {dataKey}. Data: {serializedData}");
            }
            else
            {
                Debug.Log($"[{nameof(StorageService)}] - Trying to load non-existing data. Creating a new one. Key: {dataKey}");
            }

            return data;
        }

        public bool HasKey(string key)
            => currentHandler.HasKey(key);

        public void DeleteDataByKey(string key)
            => currentHandler.DeleteDataByKey(key);

        private string GetDataKey<T>(string id)
        {
            var idPostfix = string.IsNullOrEmpty(id) ? string.Empty : $"_{id}";
            var dataKey = typeof(T).Name + idPostfix;
            return dataKey;
        }

        private void SaveSerializedData(string key, string stringData)
            => currentHandler.Save(key, stringData);

        private string Serialize<T>(T data)
            => JsonConvert.SerializeObject(data);

        private string LoadSerializedData(string key)
            => currentHandler.Load(key);

        private T Deserialize<T>(string stringData) where T : new()
            => JsonConvert.DeserializeObject<T>(stringData);
        
        private void InitSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }
    }
}