using System;
using System.Collections.Generic;

namespace Analytics
{
    [Serializable]
    public class AnalyticsEventsPackage
    {
        public List<AnalyticsEventData> events = new ();
    }
}