using System.Collections.Generic;
using System.Linq;

namespace CodeHub.Core.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ICollection<string> _screens = new LinkedList<string>();

        public void LogScreen(string screenName)
        {
            _screens.Add(screenName);
            if (_screens.Count > 25)
                _screens.Remove(_screens.First());
        }

        public IEnumerable<string> GetVisitedScreens()
        {
            return _screens.ToList().AsEnumerable();
        }
    }
}

