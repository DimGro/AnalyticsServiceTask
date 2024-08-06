using Analytics;
using UnityEngine;

namespace Main
{
    public class AnalyticsRequestsTester : MonoBehaviour
    {
        [SerializeField] private string eventType;
        [SerializeField] private string eventData;

        public void SendEvent()
        {
            AnalyticsService.Instance.TrackEvent(eventType, eventData);
        }
    }
}