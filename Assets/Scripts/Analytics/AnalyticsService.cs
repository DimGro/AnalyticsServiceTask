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
        private bool isCooldownActive;

        public static AnalyticsService Instance { get; private set; }

        private AnalyticsEventsPackage CurrentCollectingPackage
        {
            get
            {
                if (currentCollectingPackage == null)
                {
                    currentCollectingPackage = new AnalyticsEventsPackage();
                    storageData.packages.Add(currentCollectingPackage);
                    SaveStorageData();
                }

                return currentCollectingPackage;
            }
        }

        private void Awake()
        {
            InitSingleton();
        }

        private void Start()
        {
            LoadStorageData();
            TrySendCachedPackages();
        }

        public void TrackEvent(string type, string data)
        {
            var eventData = new AnalyticsEventData(type, data);

            CurrentCollectingPackage.events.Add(eventData);
            SaveStorageData();
            
            if (!isCooldownActive) StartCoroutine(SendPackageDelayed(currentCollectingPackage));
        }
        
        private IEnumerator SendPackageDelayed(AnalyticsEventsPackage package)
        {
            isCooldownActive = true;
            yield return new WaitForSeconds(cooldownBeforeSend);
            isCooldownActive = false;
            StartCoroutine(SendPackage(package));
        }

        private IEnumerator SendPackage(AnalyticsEventsPackage package)
        {
            if (package == currentCollectingPackage) currentCollectingPackage = null;

            var json = JsonConvert.SerializeObject(package);

            using var request = new UnityWebRequest(serverUrl, "POST");
            var bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"[{nameof(AnalyticsService)}] - Sending web request. Json: {json}");
            yield return request.SendWebRequest();
            Debug.Log($"[{nameof(AnalyticsService)}] - Received web request result. Result: {request.result.ToString()}. ResponseCode: {request.responseCode}. Json: {json}");

            storageData.packages.Remove(package);
            
            if (request.result != UnityWebRequest.Result.Success || request.responseCode != 200)
                MergePackage(package);
            
            SaveStorageData();
        }

        private void TrySendCachedPackages()
        {
            if (storageData.packages.Count <= 0) return;
            
            foreach (var package in storageData.packages)
                StartCoroutine(SendPackage(package));
        }

        private void MergePackage(AnalyticsEventsPackage package)
        {
            foreach (var eventData in package.events)
                CurrentCollectingPackage.events.Add(eventData);
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