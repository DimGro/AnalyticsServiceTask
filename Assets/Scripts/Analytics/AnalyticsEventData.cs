using System;

namespace Analytics
{
    [Serializable]
    public class AnalyticsEventData
    {
        public string type;
        public string data;

        public AnalyticsEventData(string type, string data)
        {
            this.type = type;
            this.data = data;
        }
    }
}