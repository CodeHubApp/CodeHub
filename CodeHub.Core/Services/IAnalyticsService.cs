using System.Collections.Generic;
using System;

namespace CodeHub.Core.Services
{
    public interface IAnalyticsService
    {
        bool Enabled { get; set; }

        void Identify(string user, IDictionary<string, string> properties = null);

        void Track(string eventName, IDictionary<string, string> properties = null);

        void Screen(string name, IDictionary<string, string> properties = null);

        void Error(Exception exception);
    }
}

