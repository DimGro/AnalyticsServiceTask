using System;
using System.Collections.Generic;

namespace Analytics
{
    [Serializable]
    public class AnalyticsStorageData
    {
        public List<AnalyticsEventsPackage> packages = new ();
    }
}