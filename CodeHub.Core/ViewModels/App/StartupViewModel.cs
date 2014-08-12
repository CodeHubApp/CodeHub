using System;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Accounts;

namespace CodeHub.Core.ViewModels.App
{
    public class StartupViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly IAccountsService _accountsService;
        private readonly ILoginService _loginService;

        public IReactiveCommand<object> GoToAccountsCommand { get; private set; }

        public IReactiveCommand<object> GoToNewUserCommand { get; private set; }

        public IReactiveCommand<object> GoToMainCommand { get; private set; }

        public IReactiveCommand BecomeActiveWindowCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        private bool _isLoggingIn;
        public bool IsLoggingIn
        {
            get { return _isLoggingIn; }
            protected set { this.RaiseAndSetIfChanged(ref _isLoggingIn, value); }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            protected set { this.RaiseAndSetIfChanged(ref _status, value); }
        }

        private Uri _imageUrl;
        public Uri ImageUrl
        {
            get { return _imageUrl; }
            protected set { this.RaiseAndSetIfChanged(ref _imageUrl, value); }
        }

        public StartupViewModel(IAccountsService accountsService, ILoginService loginService)
        {
            _accountsService = accountsService;
            _loginService = loginService;

            GoToMainCommand = ReactiveCommand.Create();
            GoToAccountsCommand = ReactiveCommand.Create();
            GoToNewUserCommand = ReactiveCommand.Create();
            BecomeActiveWindowCommand = ReactiveCommand.Create();

            GoToAccountsCommand.Subscribe(_ => ShowViewModel(CreateViewModel<AccountsViewModel>()));

            GoToNewUserCommand.Subscribe(_ => ShowViewModel(CreateViewModel<NewAccountViewModel>()));

            GoToMainCommand.Subscribe(_ => ShowViewModel(CreateViewModel<MenuViewModel>()));

            LoadCommand = ReactiveCommand.CreateAsyncTask(x => Load());
        }

        private void GoToAccountsOrNewUser()
        {
            if (_accountsService.Any())
                GoToAccountsCommand.ExecuteIfCan();
            else
                GoToNewUserCommand.ExecuteIfCan();
        }

        private async Task Load()
        {
            var account = _accountsService.GetDefault();

            // Account no longer exists
            if (account == null)
            {
                GoToAccountsOrNewUser();
            }
            else
            {
                try
                {
                    Status = string.Format("Logging in {0}", account.Username);

                    Uri avatarUri;
                    if (Uri.TryCreate(account.AvatarUrl, UriKind.Absolute, out avatarUri))
                        ImageUrl = avatarUri;

                    IsLoggingIn = true;
                    await _loginService.LoginAccount(account);
                    _accountsService.ActiveAccount = account;
                    GoToMainCommand.ExecuteIfCan();
                }
                catch
                {
                    GoToAccountsCommand.ExecuteIfCan();
                }
                finally
                {
                    IsLoggingIn = false;
                }
            }
        }
    }
}
