using System.Collections.Generic;

namespace CodeHub.Core.Services
{
    public interface IAnalyticsService
    {
        void LogScreen(string screenName);

        IEnumerable<string> GetVisitedScreens();
    }
}

