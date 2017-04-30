using System;
using CodeHub.Core.Services;
using System.Linq;
using CodeHub.Core.Factories;
using System.Windows.Input;
using Dumb = MvvmCross.Core.ViewModels;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive.Threading.Tasks;
using System.Reactive;

namespace CodeHub.Core.ViewModels.App
{
    public class StartupViewModel : BaseViewModel
    {
        private bool _isLoggingIn;
        private string _status;
        private Uri _imageUrl;
        private readonly ILoginFactory _loginFactory;
        private readonly IApplicationService _applicationService;
        private readonly IAccountsService _accountsService;

        public bool IsLoggingIn
        {
            get { return _isLoggingIn; }
            private set { this.RaiseAndSetIfChanged(ref _isLoggingIn, value); }
        }

        public string Status
        {
            get { return _status; }
            private set { this.RaiseAndSetIfChanged(ref _status, value); }
        }

        public Uri ImageUrl
        {
            get { return _imageUrl; }
            private set { this.RaiseAndSetIfChanged(ref _imageUrl, value); }
        }

        public ICommand StartupCommand
        {
            get { return new Dumb.MvxAsyncCommand(Startup); }
        }

        public Data.Account Account => _applicationService.Account;

        public ReactiveCommand<Unit, Unit> GoToMenu { get; } = ReactiveCommand.Create(() => { });

        public ReactiveCommand<Unit, Unit> GoToAccounts { get; } = ReactiveCommand.Create(() => { });

        public ReactiveCommand<Unit, Unit> GoToNewAccount { get; } = ReactiveCommand.Create(() => { });

        public StartupViewModel(
            ILoginFactory loginFactory = null, 
            IApplicationService applicationService = null,
            IAccountsService accountsService = null)
        {
            _loginFactory = loginFactory ?? GetService<ILoginFactory>();
            _applicationService = applicationService ?? GetService<IApplicationService>();
            _accountsService = accountsService ?? GetService<IAccountsService>();
        }

        protected async Task Startup()
        {
            var accounts = (await _accountsService.GetAccounts()).ToList();
            if (!accounts.Any())
            {
                GoToNewAccount.ExecuteNow();
                return;
            }

            var account = await _accountsService.GetActiveAccount();
            if (account == null)
            {
                GoToAccounts.ExecuteNow();
                return;
            }

            var isEnterprise = account.IsEnterprise || !string.IsNullOrEmpty(account.Password);

            //Lets login!
            try
            {
                ImageUrl = null;
                Status = null;
                IsLoggingIn = true;

                Uri accountAvatarUri = null;
                Uri.TryCreate(account.AvatarUrl, UriKind.Absolute, out accountAvatarUri);
                ImageUrl = accountAvatarUri;
                Status = "Logging in as " + account.Username;

                var client = await _loginFactory.LoginAccount(account);
                _applicationService.ActivateUser(account, client);

                if (!isEnterprise)
                    StarOrWatch();

                GoToMenu.ExecuteNow();
            }
            catch (GitHubSharp.UnauthorizedException e)
            {
                DisplayAlertAsync("The credentials for the selected account are incorrect. " + e.Message)
                    .ToObservable()
                    .BindCommand(GoToAccounts);
            }
            catch (Exception e)
            {
                DisplayAlert(e.Message);
                GoToAccounts.ExecuteNow();
            }
            finally
            {
                IsLoggingIn = false;
            }

        }

        private void StarOrWatch()
        {
            try
            {
                if (Settings.ShouldStar)
                {
                    Settings.ShouldStar = false;
                    var starRequest = _applicationService.Client.Users["thedillonb"].Repositories["codehub"].Star();
                    _applicationService.Client.ExecuteAsync(starRequest).ToBackground();
                }

                if (Settings.ShouldWatch)
                {
                    Settings.ShouldWatch = false;
                    var watchRequest = _applicationService.Client.Users["thedillonb"].Repositories["codehub"].Watch();
                    _applicationService.Client.ExecuteAsync(watchRequest).ToBackground();
                }
            }
            catch
            {
            }
        }
    }
}

