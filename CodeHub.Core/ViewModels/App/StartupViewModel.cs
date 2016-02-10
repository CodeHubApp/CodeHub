using System;
using CodeHub.Core.ViewModels;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using System.Linq;
using CodeHub.Core.Factories;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using System.Threading.Tasks;

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
            private set
            {
                _isLoggingIn = value;
                RaisePropertyChanged();
            }
        }

        public string Status
        {
            get { return _status; }
            private set
            {
                _status = value;
                RaisePropertyChanged();
            }
        }

        public Uri ImageUrl
        {
            get { return _imageUrl; }
            private set
            {
                _imageUrl = value;
                RaisePropertyChanged();
            }
        }

        public ICommand StartupCommand
        {
            get { return new MvxAsyncCommand(Startup); }
        }

        public StartupViewModel(ILoginFactory loginFactory, IApplicationService applicationService, IDefaultValueService defaultValueService)
        {
            _loginFactory = loginFactory;
            _applicationService = applicationService;
            _defaultValueService = defaultValueService;
        }

        protected async Task Startup()
        {
            if (!_applicationService.Accounts.Any())
            {
                ShowViewModel<Accounts.AccountsViewModel>();
                ShowViewModel<Accounts.NewAccountViewModel>();
                return;
            }

            var accounts = GetService<IAccountsService>();
            var account = accounts.GetDefault();
            if (account == null)
            {
                ShowViewModel<Accounts.AccountsViewModel>();
                return;
            }

            var isEnterprise = account.IsEnterprise || !string.IsNullOrEmpty(account.Password);
            if (account.DontRemember)
            {
                ShowViewModel<Accounts.AccountsViewModel>();

                //Hack for now
                if (isEnterprise)
                {
                    ShowViewModel<Accounts.AddAccountViewModel>(new Accounts.AddAccountViewModel.NavObject { AttemptedAccountId = account.Id });
                }
                else
                {
                    ShowViewModel<Accounts.LoginViewModel>(Accounts.LoginViewModel.NavObject.CreateDontRemember(account));
                }

                return;
            }

            //Lets login!
            try
            {
                IsLoggingIn = true;

                Uri accountAvatarUri = null;
                Uri.TryCreate(account.AvatarUrl, UriKind.Absolute, out accountAvatarUri);
                ImageUrl = accountAvatarUri;
                Status = "Logging in as " + account.Username;

                var client = await _loginFactory.LoginAccount(account);
                _applicationService.ActivateUser(account, client);

                if (!isEnterprise)
                    StarOrWatch();
            }
            catch (GitHubSharp.UnauthorizedException e)
            {
                DisplayAlert("The credentials for the selected account are incorrect. " + e.Message);

                ShowViewModel<Accounts.AccountsViewModel>();
                if (isEnterprise)
                    ShowViewModel<Accounts.AddAccountViewModel>(new Accounts.AddAccountViewModel.NavObject { AttemptedAccountId = account.Id });
                else
                    ShowViewModel<Accounts.LoginViewModel>(Accounts.LoginViewModel.NavObject.CreateDontRemember(account));

                StarOrWatch();
            }
            catch (Exception e)
            {
                DisplayAlert(e.Message);
                ShowViewModel<Accounts.AccountsViewModel>();
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

