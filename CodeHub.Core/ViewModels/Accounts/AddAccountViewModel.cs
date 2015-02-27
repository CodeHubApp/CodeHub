using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Data;
using ReactiveUI;
using CodeHub.Core.Services;
using CodeHub.Core.Messages;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class AddAccountViewModel : BaseViewModel 
    {
        public bool IsEnterprise { get; set; }

        public string TwoFactor { get; set; }

        public IReactiveCommand LoginCommand { get; private set; }

        private string _username;
        public string Username
        {
            get { return _username; }
            set { this.RaiseAndSetIfChanged(ref _username, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { this.RaiseAndSetIfChanged(ref _password, value); }
        }

        private string _domain;
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

        public AddAccountViewModel(ILoginService loginFactory, IAccountsRepository accountsService)
        {
            Title = "Login";

            this.WhenAnyValue(x => x.AttemptedAccount).Where(x => x != null).Subscribe(x =>
            {
                Username = x.Username;
                Password = x.Password;
                Domain = x.Domain;
            });

            var canLogin = this.WhenAnyValue(x => x.Username, y => y.Password, 
                (x, y) => !string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(y));

            var loginCommand = ReactiveCommand.CreateAsyncTask(canLogin, async _ => 
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

                var loginData = await loginFactory.Authenticate(apiUrl, Username, Password, TwoFactor, IsEnterprise, _attemptedAccount);
                await loginFactory.LoginAccount(loginData.Account);
                accountsService.SetDefault(loginData.Account);
                return loginData.Account;
            });

            loginCommand.Subscribe(x => MessageBus.Current.SendMessage(new LogoutMessage()));
            LoginCommand = loginCommand;
        }
    }
}
