using System;
using CodeHub.Core.ViewModels;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using System.Linq;
using CodeHub.Core.Factories;
using System.Windows.Input;
using Dumb = MvvmCross.Core.ViewModels;
using System.Threading.Tasks;
using ReactiveUI;
using CodeHub.Core.ViewModels.Accounts;
using System.Reactive.Threading.Tasks;

namespace CodeHub.Core.ViewModels.App
{
    public class StartupViewModel : BaseViewModel
    {
        private bool _isLoggingIn;
        private string _status;
        private Uri _imageUrl;
        private readonly ILoginFactory _loginFactory;
        private readonly IApplicationService _applicationService;
        private readonly IDefaultValueService _defaultValueService;

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

        public ReactiveCommand<object> GoToMenu { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> GoToAccounts { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> GoToNewAccount { get; } = ReactiveCommand.Create();

        public StartupViewModel(
            ILoginFactory loginFactory, 
            IApplicationService applicationService, 
            IDefaultValueService defaultValueService)
        {
            _loginFactory = loginFactory;
            _applicationService = applicationService;
            _defaultValueService = defaultValueService;
        }

        protected async Task Startup()
        {
            if (!_applicationService.Accounts.Any())
            {
                GoToNewAccount.Execute(null);
                return;
            }

            var accounts = GetService<IAccountsService>();
            var account = accounts.GetDefault();
            if (account == null)
            {
                GoToAccounts.Execute(null);
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

                GoToMenu.Execute(typeof(MenuViewModel));
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
                GoToAccounts.Execute(null);
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
                bool shouldStar;
                if (_defaultValueService.TryGet("SHOULD_STAR_CODEHUB", out shouldStar) && shouldStar)
                {
                    _defaultValueService.Clear("SHOULD_STAR_CODEHUB");
                    var starRequest = _applicationService.Client.Users["thedillonb"].Repositories["codehub"].Star();
                    _applicationService.Client.ExecuteAsync(starRequest).ToBackground();
                }

                bool shouldWatch;
                if (_defaultValueService.TryGet("SHOULD_WATCH_CODEHUB", out shouldWatch) && shouldWatch)
                {
                    _defaultValueService.Clear("SHOULD_WATCH_CODEHUB");
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

