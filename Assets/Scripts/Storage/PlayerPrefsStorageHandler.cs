using UnityEngine;

namespace Storage
{
    public class PlayerPrefsStorageHandler : IStorageHandler
    {
        public void Save(string key, string value) 
            => PlayerPrefs.SetString(key, value);

        public string Load(string key)
        {
            var result = string.Empty;
            if (HasKey(key)) result = PlayerPrefs.GetString(key);
            return result;
        }

        public bool HasKey(string key) 
            => PlayerPrefs.HasKey(key);

        public void DeleteDataByKey(string key)
        {
            if (HasKey(key)) PlayerPrefs.DeleteKey(key);
        }
    }
}