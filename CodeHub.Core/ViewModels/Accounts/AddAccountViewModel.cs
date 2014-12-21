using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Data;
using ReactiveUI;
using CodeHub.Core.Services;
using CodeHub.Core.Messages;
using Xamarin.Utilities.ViewModels;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class AddAccountViewModel : BaseViewModel 
    {
        private readonly ILoginService _loginFactory;
        private string _username;
        private string _password;
        private string _domain;

        public bool IsEnterprise { get; set; }

        public string TwoFactor { get; set; }

        public IReactiveCommand LoginCommand { get; private set; }

        public string Username
        {
            get { return _username; }
            set { this.RaiseAndSetIfChanged(ref _username, value); }
        }

        public string Password
        {
            get { return _password; }
            set { this.RaiseAndSetIfChanged(ref _password, value); }
        }

        public string Domain
        {
            get { return _domain; }
            set { this.RaiseAndSetIfChanged(ref _domain, value); }
        }

        private GitHubAccount _attemptedAccount;
        public GitHubAccount AttemptedAccount
        {
            get { return _attemptedAccount; }
            set { this.RaiseAndSetIfChanged(ref _attemptedAccount, value); }
        }

        public AddAccountViewModel(ILoginService loginFactory, IAccountsService accountsService)
        {
            Title = "Login";

            _loginFactory = loginFactory;

            this.WhenAnyValue(x => x.AttemptedAccount).Where(x => x != null).Subscribe(x =>
            {
                Username = x.Username;
                Password = x.Password;
                Domain = x.Domain;
            });

            var loginCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Username, y => y.Password, (x, y) => !string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(y)),
                _ => Login());
            loginCommand.Subscribe(x => accountsService.ActiveAccount = x);
            loginCommand.Subscribe(x => MessageBus.Current.SendMessage(new LogoutMessage()));
            LoginCommand = loginCommand;
        }

		private async Task<GitHubAccount> Login()
        {
            var apiUrl = IsEnterprise ? Domain : null;
            if (apiUrl != null)
            {
                if (!apiUrl.StartsWith("http://") && !apiUrl.StartsWith("https://"))
                    apiUrl = "https://" + apiUrl;
                if (!apiUrl.EndsWith("/"))
                    apiUrl += "/";
                if (!apiUrl.Contains("/api/"))
                    apiUrl += "api/v3/";
            }

            var loginData = await _loginFactory.Authenticate(apiUrl, Username, Password, TwoFactor, IsEnterprise, _attemptedAccount);
			await _loginFactory.LoginAccount(loginData.Account);
            return loginData.Account;
        }
    }
}
