using CodeFramework.Core.Services;
using CodeHub.Core.Services;
using System;
using ReactiveUI;
using Xamarin.Utilities.Core.Services;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.App
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly IFeaturesService _featuresService;
        private readonly IDefaultValueService _defaultValueService;
        private readonly IAccountsService _accountsService;
        private readonly IAnalyticsService _analyticsService;

        public IReactiveCommand<object> GoToDefaultStartupViewCommand { get; private set; }

        public IReactiveCommand DeleteAllCacheCommand { get; private set; }

        public string DefaultStartupViewName
        {
            get { return _applicationService.Account.DefaultStartupView; }
        }

        public bool AnalyticsEnabled
        {
            get { return _analyticsService.Enabled; }
            set
            {
                if (value == AnalyticsEnabled)
                    return;
                _analyticsService.Enabled = value;
                this.RaisePropertyChanged();
            }
        }

        public SettingsViewModel(IApplicationService applicationService, IFeaturesService featuresService, 
                                 IDefaultValueService defaultValueService, IAccountsService accountsService,
                                 IAnalyticsService analyticsService)
        {
            _applicationService = applicationService;
            _featuresService = featuresService;
            _defaultValueService = defaultValueService;
            _accountsService = accountsService;
            _analyticsService = analyticsService;

            GoToDefaultStartupViewCommand = ReactiveCommand.Create();
            GoToDefaultStartupViewCommand.Subscribe(_ => ShowViewModel(CreateViewModel<DefaultStartupViewModel>()));

            DeleteAllCacheCommand = ReactiveCommand.Create();
        }


        public bool LargeFonts
        {
            get 
            { 
                bool value;
                _defaultValueService.TryGet("large_fonts", out value);
                return value;
            }
            set { _defaultValueService.Set("large_fonts", value); }
        }

        public bool PushNotificationsActivated
        {
            get { return _featuresService.IsPushNotificationsActivated; }
        }

        public bool PushNotificationsEnabled
		{
            get 
            { 
                return PushNotificationsActivated && _applicationService.Account.IsPushNotificationsEnabled.HasValue && _applicationService.Account.IsPushNotificationsEnabled.Value; 
            }
            set 
            { 
                if (!PushNotificationsActivated)
                    throw new Exception("Push notifications have not been activated");
                RegisterPushNotifications(value);
            }
		}

		private async void RegisterPushNotifications(bool enabled)
		{
			var notificationService = GetService<IPushNotificationsService>();

			try
			{
				if (enabled)
                {
					await notificationService.Register();
                }
				else
                {
                    await notificationService.Deregister();
                }

				_applicationService.Account.IsPushNotificationsEnabled = enabled;
				_accountsService.Update(_applicationService.Account);
			}
			catch (Exception e)
			{
                System.Diagnostics.Debug.WriteLine("Unable to register push notifications: " + e.Message);
			}
		}

		public float CacheSize
		{
			get
			{
//				if (_applicationService.Account.Cache == null)
//					return 0f;
//
//				var totalCacheSize = _applicationService.Account.Cache.Sum(x => System.IO.File.Exists(x.Path) ? new System.IO.FileInfo(x.Path).Length : 0);
//				var totalCacheSizeMB = ((float)totalCacheSize / 1024f / 1024f);
//				return totalCacheSizeMB;
			    return 0;
			}
		}
    }
}
