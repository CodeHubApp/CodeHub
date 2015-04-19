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
using CodeHub.Core.Messages;
using System.Reactive.Threading.Tasks;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.App
{
    public class StartupViewModel : BaseViewModel
    {
        private readonly IAccountsRepository _accountsService;
        private readonly ISessionService _sessionService;
        private readonly IAlertDialogFactory _alertDialogFactory;

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

        private Uri _imageUrl;
        public Uri ImageUrl
        {
            get { return _imageUrl; }
            private set { this.RaiseAndSetIfChanged(ref _imageUrl, value); }
        }

        public StartupViewModel(
            ISessionService sessionService, 
            IAccountsRepository accountsService, 
            IAlertDialogFactory alertDialogFactory)
        {
            _sessionService = sessionService;
            _accountsService = accountsService;
            _alertDialogFactory = alertDialogFactory;

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
                MessageBus.Current.SendMessage(new LoggingInMessage(account));

                Uri avatarUri;
                if (Uri.TryCreate(account.AvatarUrl, UriKind.Absolute, out avatarUri))
                    ImageUrl = avatarUri;

                IsLoggingIn = true;
                await _sessionService.SetSessionAccount(account);
                NavigateTo(this.CreateViewModel<MenuViewModel>());
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
    }
}
