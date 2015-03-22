using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace CodeHub.Core.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        public bool Enabled { get; set; }

        public AnalyticsService()
        {
            Enabled = true;
        }

        public void Error(Exception exception)
        {
            if (!Enabled)
                return;

            Debug.WriteLine("[Analytics]: Error {0}", exception.Message);
            Flurry.Analytics.Portable.AnalyticsApi.LogError(exception);
        }

        public void Identify(string user, IDictionary<string, string> properties = null)
        {
            if (!Enabled)
                return;
            
            Debug.WriteLine("[Analytics]: Identified {0}", user);
            Flurry.Analytics.Portable.AnalyticsApi.SetUserId(user);
        }

        public void Track(string eventName, IDictionary<string, string> properties = null)
        {
            if (!Enabled)
                return;
            
            Debug.WriteLine("[Analytics]: tracked {0}", eventName);
            Flurry.Analytics.Portable.AnalyticsApi.LogEvent("event:" + eventName);
        }

        public void Screen(string name, IDictionary<string, string> properties = null)
        {
            if (!Enabled)
                return;
            
            Debug.WriteLine("[Analytics]: opened {0}", name);
            Flurry.Analytics.Portable.AnalyticsApi.LogPageView();
            Flurry.Analytics.Portable.AnalyticsApi.LogEvent("view:" + name);
        }
    }
}

