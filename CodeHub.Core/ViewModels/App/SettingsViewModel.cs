using CodeHub.Core.Services;
using System;
using ReactiveUI;
using Xamarin.Utilities.Core.Services;
using Xamarin.Utilities.Core.ViewModels;
using CodeHub.Core.ViewModels.Repositories;

namespace CodeHub.Core.ViewModels.App
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly IFeaturesService _featuresService;
        private readonly IDefaultValueService _defaultValueService;
        private readonly IAccountsService _accountsService;
        private readonly IAnalyticsService _analyticsService;
        private readonly IEnvironmentalService _environmentService;

        public IReactiveCommand<object> GoToDefaultStartupViewCommand { get; private set; }

        public IReactiveCommand DeleteAllCacheCommand { get; private set; }

        public string DefaultStartupViewName
        {
            get { return _applicationService.Account.DefaultStartupView; }
        }

        public string Version
        {
            get { return _environmentService.ApplicationVersion; }
        }

        public IReactiveCommand GoToSourceCodeCommand { get; private set; }

        public SettingsViewModel(IApplicationService applicationService, IFeaturesService featuresService, 
                                 IDefaultValueService defaultValueService, IAccountsService accountsService,
                                 IAnalyticsService analyticsService, IEnvironmentalService environmentalService)
        {
            _applicationService = applicationService;
            _featuresService = featuresService;
            _defaultValueService = defaultValueService;
            _accountsService = accountsService;
            _analyticsService = analyticsService;
            _environmentService = environmentalService;

            GoToDefaultStartupViewCommand = ReactiveCommand.Create();
            GoToDefaultStartupViewCommand.Subscribe(_ => ShowViewModel(CreateViewModel<DefaultStartupViewModel>()));

            DeleteAllCacheCommand = ReactiveCommand.Create();

            GoToSourceCodeCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = "thedillonb";
                vm.RepositoryName = "codehub";
                ShowViewModel(vm);
            });
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
