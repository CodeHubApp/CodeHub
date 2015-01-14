using System;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Accounts;
using System.Reactive;

namespace CodeHub.Core.ViewModels.App
{
    public class StartupViewModel : BaseViewModel
    {
        private readonly IAccountsService _accountsService;
        private readonly ILoginService _loginService;

        public IReactiveCommand<Unit> StartupCommand { get; private set; }

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

            StartupCommand = ReactiveCommand.CreateAsyncTask(x => Load());
        }

        private void GoToAccountsOrNewUser()
        {
            if (_accountsService.Any())
                NavigateTo(this.CreateViewModel<AccountsViewModel>());
            else
                NavigateTo(this.CreateViewModel<NewAccountViewModel>());
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
                    NavigateTo(this.CreateViewModel<MenuViewModel>());
                }
                catch
                {
                    NavigateTo(this.CreateViewModel<AccountsViewModel>());
                }
                finally
                {
                    IsLoggingIn = false;
                }
            }
        }
    }
}
