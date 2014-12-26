using CodeHub.Core.Services;
using System;
using ReactiveUI;
using CodeHub.Core.ViewModels.Repositories;
using Xamarin.Utilities.ViewModels;
using Xamarin.Utilities.Services;
using System.Reactive.Linq;
using Humanizer;

namespace CodeHub.Core.ViewModels.Settings
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly IFeaturesService _featuresService;
        private readonly IAccountsService _accountsService;
        private readonly IEnvironmentalService _environmentService;
        private readonly IPushNotificationsService _pushNotificationsService;

        public IReactiveCommand<object> GoToDefaultStartupViewCommand { get; private set; }

        public IReactiveCommand DeleteAllCacheCommand { get; private set; }

        public string DefaultStartupViewName
        {
            get { return _applicationService.Account.DefaultStartupView ?? "Default"; }
            private set { this.RaisePropertyChanged(); }
        }

        public string SyntaxHighlighter
        {
            get 
            { 
                const string @default = "Default";
                if (_applicationService.Account.CodeEditTheme != null)
                    return _applicationService.Account.CodeEditTheme.Humanize(LetterCasing.Title) ?? @default;
                else
                    return @default;
            }
            private set { this.RaisePropertyChanged(); }
        }

        public string Version
        {
            get { return _environmentService.ApplicationVersion; }
        }

        public string OSVersion
        {
            get { return _environmentService.OSVersion; }
        }

        private string _accountImageUrl;
        public string AccountImageUrl
        {
            get { return _accountImageUrl; }
            set { this.RaiseAndSetIfChanged(ref _accountImageUrl, value); }
        }

        private bool _showOrganizationsInEvents;
        public bool ShowOrganizationsInEvents
        {
            get { return _showOrganizationsInEvents; }
            set { this.RaiseAndSetIfChanged(ref _showOrganizationsInEvents, value); }
        }

        private bool _expandOrganizations;
        public bool ExpandOrganizations
        {
            get { return _expandOrganizations; }
            set { this.RaiseAndSetIfChanged(ref _expandOrganizations, value); }
        }

        private bool _showRepositoryDescriptionInList;
        public bool ShowRepositoryDescriptionInList
        {
            get { return _showRepositoryDescriptionInList; }
            set { this.RaiseAndSetIfChanged(ref _showRepositoryDescriptionInList, value); }
        }

        private bool _saveCredentials;
        public bool SaveCredentials
        {
            get { return _saveCredentials; }
            set { this.RaiseAndSetIfChanged(ref _saveCredentials, value); }
        }

        public IReactiveCommand GoToSourceCodeCommand { get; private set; }

        public IReactiveCommand GoToSyntaxHighlighterCommand { get; private set; }

        public SettingsViewModel(IApplicationService applicationService, IFeaturesService featuresService, 
            IAccountsService accountsService, IEnvironmentalService environmentalService, 
            IPushNotificationsService pushNotificationsService)
        {
            Title = "Account Settings";

            _applicationService = applicationService;
            _featuresService = featuresService;
            _accountsService = accountsService;
            _environmentService = environmentalService;
            _pushNotificationsService = pushNotificationsService;

            AccountImageUrl = _accountsService.ActiveAccount.AvatarUrl;

            GoToDefaultStartupViewCommand = ReactiveCommand.Create();
            GoToDefaultStartupViewCommand.Subscribe(_ => 
            {
                var vm = this.CreateViewModel<DefaultStartupViewModel>();
                vm.WhenAnyValue(x => x.SelectedStartupView).Subscribe(x => 
                    DefaultStartupViewName = x);
                NavigateTo(vm);
            });

            GoToSyntaxHighlighterCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<SyntaxHighlighterSettingsViewModel>();
                vm.SaveCommand.Subscribe(__ => SyntaxHighlighter = vm.SelectedTheme);
                NavigateTo(vm);
            });

            DeleteAllCacheCommand = ReactiveCommand.Create();

            GoToSourceCodeCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = "thedillonb";
                vm.RepositoryName = "codehub";
                NavigateTo(vm);
            });

            ShowOrganizationsInEvents = accountsService.ActiveAccount.ShowOrganizationsInEvents;
            this.WhenAnyValue(x => x.ShowOrganizationsInEvents).Skip(1).Subscribe(x =>
            {
                accountsService.ActiveAccount.ShowOrganizationsInEvents = x;
                accountsService.Update(accountsService.ActiveAccount);
            });

            ExpandOrganizations = accountsService.ActiveAccount.ExpandOrganizations;
            this.WhenAnyValue(x => x.ExpandOrganizations).Skip(1).Subscribe(x =>
            {
                accountsService.ActiveAccount.ExpandOrganizations = x;
                accountsService.Update(accountsService.ActiveAccount);
            });

            ShowRepositoryDescriptionInList = accountsService.ActiveAccount.ShowRepositoryDescriptionInList;
            this.WhenAnyValue(x => x.ShowRepositoryDescriptionInList).Skip(1).Subscribe(x =>
            {
                accountsService.ActiveAccount.ShowRepositoryDescriptionInList = x;
                accountsService.Update(accountsService.ActiveAccount);
            });

            SaveCredentials = accountsService.ActiveAccount.SaveCredentails;
            this.WhenAnyValue(x => x.SaveCredentials).Skip(1).Subscribe(x =>
            {
                accountsService.ActiveAccount.SaveCredentails = x;
                accountsService.Update(accountsService.ActiveAccount);
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
			try
			{
				if (enabled)
                {
                    await _pushNotificationsService.Register();
                }
				else
                {
                    await _pushNotificationsService.Deregister();
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
