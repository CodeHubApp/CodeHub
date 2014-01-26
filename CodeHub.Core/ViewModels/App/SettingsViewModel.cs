using CodeFramework.Core.ViewModels;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using System.Linq;
using CodeFramework.Core.Services;
using CodeHub.Core.Services;
using System;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.App
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IFeaturesService _featuresService;

        public SettingsViewModel(IFeaturesService featuresService)
        {
            _featuresService = featuresService;
        }

		public string DefaultStartupViewName
		{
			get { return this.GetApplication().Account.DefaultStartupView; }
		}

		public ICommand GoToDefaultStartupViewCommand
		{
			get { return new MvxCommand(() => ShowViewModel<DefaultStartupViewModel>()); }
		}

		public ICommand DeleteAllCacheCommand
		{
			get { return new MvxCommand(DeleteCache); }
		}

		public bool AnalyticsEnabled
		{
			get
			{
				return GetService<IAnalyticsService>().Enabled;
			}
			set
			{
				GetService<IAnalyticsService>().Enabled = value;
			}
		}

        public bool PushNotificationsActivated
        {
            get { return _featuresService.IsPushNotificationsActivated; }
        }

        public bool PushNotificationsEnabled
		{
            get { return this.GetApplication().Account.PushNotificationsEnabled; }
            set 
            { 
                if (PushNotificationsActivated)
                    RegisterPushNotifications(value); 
            }
		}

		private async Task RegisterPushNotifications(bool enabled)
		{
			var notificationService = GetService<IPushNotificationsService>();

			try
			{
				if (enabled)
					await Task.Run(() => notificationService.Register());
				else
					await Task.Run(() => notificationService.Deregister());
				this.GetApplication().Account.PushNotificationsEnabled = enabled;
				this.GetApplication().Accounts.Update(this.GetApplication().Account);
				RaisePropertyChanged(() => PushNotificationsEnabled);
			}
			catch (Exception e)
			{
				ReportError(e);
				RaisePropertyChanged(() => PushNotificationsEnabled);
			}
		}

		private void DeleteCache()
		{
			if (this.GetApplication().Account.Cache != null)
				this.GetApplication().Account.Cache.DeleteAll();
		}

		public float CacheSize
		{
			get
			{
				if (this.GetApplication().Account.Cache == null)
					return 0f;

				var totalCacheSize = this.GetApplication().Account.Cache.Sum(x => System.IO.File.Exists(x.Path) ? new System.IO.FileInfo(x.Path).Length : 0);
				var totalCacheSizeMB = ((float)totalCacheSize / 1024f / 1024f);
				return totalCacheSizeMB;
			}
		}
    }
}
