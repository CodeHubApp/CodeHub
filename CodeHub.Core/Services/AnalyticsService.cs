using System;
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
        }

        public void Identify(string user, IDictionary<string, string> properties = null)
        {
            if (!Enabled)
                return;
        }

        public void Track(string eventName, IDictionary<string, string> properties = null)
        {
            if (!Enabled)
                return;
        }

        public void Screen(string name, IDictionary<string, string> properties = null)
        {
            if (!Enabled)
                return;
        }
    }
}

