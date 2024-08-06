using System.Collections;
using Newtonsoft.Json;
using Storage;
using UnityEngine;
using UnityEngine.Networking;

namespace Analytics
{
    public class AnalyticsService : MonoBehaviour
    {
        [SerializeField] private string serverUrl;
        [SerializeField] private float cooldownBeforeSend;
        
        private AnalyticsStorageData storageData;
        private AnalyticsEventsPackage currentCollectingPackage;

        public static AnalyticsService Instance { get; private set; }

        private void Awake()
        {
            InitSingleton();
        }

        private void Start()
        {
            LoadStorageData();
            
            foreach (var package in storageData.packages)
                StartCoroutine(SendPackages(package));
        }

        public void TrackEvent(string type, string data)
        {
            var eventData = new AnalyticsEventData(type, data);
            
            if (currentCollectingPackage == null)
            {
                currentCollectingPackage = new AnalyticsEventsPackage();
                storageData.packages.Add(currentCollectingPackage);
            }
            
            currentCollectingPackage.events.Add(eventData);
            SaveStorageData();

            StartCoroutine(SendPackageDelayed(currentCollectingPackage));
        }
        
        private IEnumerator SendPackageDelayed(AnalyticsEventsPackage package)
        {
            yield return new WaitForSeconds(cooldownBeforeSend);
            StartCoroutine(SendPackages(package));
        }

        private IEnumerator SendPackages(AnalyticsEventsPackage package)
        {
            if (package == currentCollectingPackage) currentCollectingPackage = null;

            var json = JsonConvert.SerializeObject(package.events);

            var request = new UnityWebRequest(serverUrl, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
            {
                storageData.packages.Remove(package);
                SaveStorageData();
            }
            else
            {
                StartCoroutine(SendPackages(package));
            }
        }

        private void LoadStorageData() => storageData = StorageService.Instance.LoadData<AnalyticsStorageData>();
        
        private void SaveStorageData() => StorageService.Instance.SaveData(storageData);

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