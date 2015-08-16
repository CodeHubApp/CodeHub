using System;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Accounts;
using System.Reactive;
using CodeHub.Core.Data;
using CodeHub.Core.Factories;
using GitHubSharp;
using System.Reactive.Threading.Tasks;
using System.Reactive.Linq;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.ViewModels.App
{
    public class StartupViewModel : BaseViewModel
    {
        private readonly IAccountsRepository _accountsService;
        private readonly ISessionService _sessionService;
        private readonly IAlertDialogFactory _alertDialogFactory;
        private readonly IDefaultValueService _defaultValueService;

        public IReactiveCommand<Unit> StartupCommand { get; private set; }

        private bool _isLoggingIn;
        public bool IsLoggingIn
        {
            get { return _isLoggingIn; }
            private set { this.RaiseAndSetIfChanged(ref _isLoggingIn, value); }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            private set { this.RaiseAndSetIfChanged(ref _status, value); }
        }

        private GitHubAvatar _avatar;
        public GitHubAvatar Avatar
        {
            get { return _avatar; }
            private set { this.RaiseAndSetIfChanged(ref _avatar, value); }
        }

        public StartupViewModel(
            ISessionService sessionService, 
            IAccountsRepository accountsService, 
            IAlertDialogFactory alertDialogFactory,
            IDefaultValueService defaultValueService)
        {
            _sessionService = sessionService;
            _accountsService = accountsService;
            _alertDialogFactory = alertDialogFactory;
            _defaultValueService = defaultValueService;

            StartupCommand = ReactiveCommand.CreateAsyncTask(x => Load());
        }

        private async Task GoToAccountsOrNewUser()
        {
            var accounts = await _accountsService.GetAll();
            if (accounts.Any())
                NavigateTo(this.CreateViewModel<AccountsViewModel>());
            else
                NavigateTo(this.CreateViewModel<NewAccountViewModel>());
        }

        private async Task Load()
        {
            var account = await _accountsService.GetDefault();

            // Account no longer exists
            if (account == null)
            {
                await GoToAccountsOrNewUser();
                return;
            }

            try
            {
                Status = string.Format("Logging in {0}", account.Username);
                Avatar = new GitHubAvatar(account.AvatarUrl);

                IsLoggingIn = true;
                await _sessionService.SetSessionAccount(account);
                NavigateTo(this.CreateViewModel<MenuViewModel>());
                StarOrWatch();
            }
            catch (UnauthorizedException)
            {
                _alertDialogFactory.Alert("Unable to login!", "Your credentials are no longer valid for this account.")
                    .ToObservable()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => NavigateTo(this.CreateViewModel<AccountsViewModel>()));
            }
            catch (Exception e)
            {
                _alertDialogFactory.Alert("Unable to login!", e.Message)
                    .ToObservable()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => NavigateTo(this.CreateViewModel<AccountsViewModel>()));
            }
            finally
            {
                IsLoggingIn = false;
            }
        }

        private async Task StarOrWatch()
        {
            bool shouldStar;
            if (_defaultValueService.TryGet<bool>("SHOULD_STAR_CODEHUB", out shouldStar) && shouldStar)
            {
                _defaultValueService.Set("SHOULD_STAR_CODEHUB", false);
                await _sessionService.GitHubClient.Activity.Starring.StarRepo("thedillonb", "codehub");
            }

            bool shouldWatch;
            if (_defaultValueService.TryGet<bool>("SHOULD_WATCH_CODEHUB", out shouldWatch) && shouldWatch)
            {
                _defaultValueService.Set("SHOULD_WATCH_CODEHUB", false);
                await _sessionService.GitHubClient.Activity.Watching.WatchRepo("thedillonb", "codehub", new Octokit.NewSubscription { Subscribed = true });
            }
        }
    }
}
