using System;
using CodeFramework.Core.Services;


namespace CodeFramework.iOS.Services
{
	public class AnalyticsService : IAnalyticsService
    {
        private bool _enabled;

        public void Init(string id, string key)
		{
		}

		public bool Enabled
		{
			get
            {
                return _enabled;
            }
			set
            {
                _enabled = value;
                MonoTouch.Utilities.Defaults.SetBool(_enabled, "CodeFramework.Analytics");
                MonoTouch.Utilities.Defaults.Synchronize();
            }
		}
    }
}

