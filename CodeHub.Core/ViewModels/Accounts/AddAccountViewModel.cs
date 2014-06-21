using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeFramework.Core.Services;
using CodeHub.Core.Data;
using CodeHub.Core.Factories;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class AddAccountViewModel : BaseViewModel 
    {
        private readonly ILoginFactory _loginFactory;
        private readonly IAccountsService _accountsService;
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

        public AddAccountViewModel(ILoginFactory loginFactory, IAccountsService accountsService)
        {
            _loginFactory = loginFactory;
            _accountsService = accountsService;

            this.WhenAnyValue(x => x.AttemptedAccount).Where(x => x != null).Subscribe(x =>
            {
                Username = x.Username;
                Password = x.Password;
                Domain = x.Domain;
            });

            LoginCommand = new ReactiveCommand(this.WhenAnyValue(x => x.Username, y => y.Password, (x, y) => !string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(y)));
            LoginCommand.RegisterAsyncTask(_ => Login());
        }

		private async Task Login()
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

            try
            {
				Console.WriteLine(apiUrl);
                var loginData = await _loginFactory.Authenticate(apiUrl, Username, Password, TwoFactor, IsEnterprise, _attemptedAccount);
				await _loginFactory.LoginAccount(loginData.Account);
                _accountsService.ActiveAccount = loginData.Account;
            }
            catch (Exception e)
            {
                TwoFactor = null;

                // Don't log an error for a two factor warning
                if (!(e is LoginFactory.TwoFactorRequiredException))
                {
                    Password = null;
                    throw;
                }
            }
        }
    }
}
