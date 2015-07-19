using CodeHub.Core.Services;
using System;
using ReactiveUI;
using CodeHub.Core.ViewModels.Repositories;
using Xamarin.Utilities.Services;
using System.Reactive.Linq;
using Humanizer;
using CodeHub.Core.Data;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Settings
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ISessionService _sessionService;
        private readonly IFeaturesService _featuresService;
        private readonly IAccountsRepository _accountsService;
        private readonly IEnvironmentalService _environmentService;
        private readonly IPushNotificationRegistrationService _pushNotificationsService;

        public IReactiveCommand<object> GoToDefaultStartupViewCommand { get; private set; }

        public IReactiveCommand DeleteAllCacheCommand { get; private set; }

        public string DefaultStartupViewName
        {
            get { return _sessionService.Account.DefaultStartupView ?? "Default"; }
            private set { this.RaisePropertyChanged(); }
        }

        public string SyntaxHighlighter
        {
            get 
            { 
                const string @default = "Default";
                if (_sessionService.Account.CodeEditTheme != null)
                    return _sessionService.Account.CodeEditTheme.Humanize(LetterCasing.Title) ?? @default;
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

        public IReactiveCommand GoToSourceCodeCommand { get; private set; }

        public IReactiveCommand GoToSyntaxHighlighterCommand { get; private set; }

        public SettingsViewModel(ISessionService applicationService, IFeaturesService featuresService, 
            IAccountsRepository accountsService, IEnvironmentalService environmentalService, 
            IPushNotificationRegistrationService pushNotificationsService)
        {
            Title = "Account Settings";

            _sessionService = applicationService;
            _featuresService = featuresService;
            _accountsService = accountsService;
            _environmentService = environmentalService;
            _pushNotificationsService = pushNotificationsService;

            AccountImageUrl = applicationService.Account.AvatarUrl;

            GoToDefaultStartupViewCommand = ReactiveCommand.Create();
            GoToDefaultStartupViewCommand.Subscribe(_ => 
            {
                var vm = this.CreateViewModel<DefaultStartupViewModel>();
                vm.WhenAnyValue(x => x.SelectedStartupView)
                    .Subscribe(x => DefaultStartupViewName = x);
                NavigateTo(vm);
            });

            GoToSyntaxHighlighterCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<SyntaxHighlighterViewModel>();
                vm.SaveCommand.Subscribe(__ => SyntaxHighlighter = vm.SelectedTheme);
                NavigateTo(vm);
            });

            DeleteAllCacheCommand = ReactiveCommand.Create();

            GoToSourceCodeCommand = ReactiveCommand.Create().WithSubscription(_ => {
                var vm = this.CreateViewModel<RepositoryViewModel>();
                vm.Init("thedillonb", "codehub");
                NavigateTo(vm);
            });

            ShowOrganizationsInEvents = applicationService.Account.ShowOrganizationsInEvents;
            this.WhenAnyValue(x => x.ShowOrganizationsInEvents).Skip(1).Subscribe(x =>
            {
                applicationService.Account.ShowOrganizationsInEvents = x;
                accountsService.Update(applicationService.Account);
            });

            ExpandOrganizations = applicationService.Account.ExpandOrganizations;
            this.WhenAnyValue(x => x.ExpandOrganizations).Skip(1).Subscribe(x =>
            {
                applicationService.Account.ExpandOrganizations = x;
                accountsService.Update(applicationService.Account);
            });

            ShowRepositoryDescriptionInList = applicationService.Account.ShowRepositoryDescriptionInList;
            this.WhenAnyValue(x => x.ShowRepositoryDescriptionInList).Skip(1).Subscribe(x =>
            {
                applicationService.Account.ShowRepositoryDescriptionInList = x;
                accountsService.Update(applicationService.Account);
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
                return PushNotificationsActivated && _sessionService.Account.IsPushNotificationsEnabled.HasValue && _sessionService.Account.IsPushNotificationsEnabled.Value; 
            }
            set 
            { 
                if (!PushNotificationsActivated)
                    throw new Exception("Push notifications have not been activated");
                RegisterPushNotifications(value).ToBackground();
            }
		}

        private async Task RegisterPushNotifications(bool enabled)
		{
			if (enabled)
            {
                await _pushNotificationsService.Register(_sessionService.Account);
            }
			else
            {
                await _pushNotificationsService.Deregister(_sessionService.Account);
            }

			_sessionService.Account.IsPushNotificationsEnabled = enabled;
			await _accountsService.Update(_sessionService.Account);
		}
    }
}
