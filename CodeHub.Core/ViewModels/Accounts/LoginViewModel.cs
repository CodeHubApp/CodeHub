using System;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using System.Threading.Tasks;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
using System.Reactive.Linq;
using CodeHub.Core.Messages;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class LoginViewModel : BaseViewModel
    {
        public const string ClientId = "72f4fb74bdba774b759d";
        public const string ClientSecret = "9253ab615f8c00738fff5d1c665ca81e581875cb";
        public static readonly string RedirectUri = "http://dillonbuchanan.com/";
        private readonly ILoginService _loginFactory;

        public string LoginUrl
        {
            get
            {
                var web = WebDomain.TrimEnd('/');
                return string.Format(
                    web + "/login/oauth/authorize?client_id={0}&redirect_uri={1}&scope={2}", 
                    ClientId, 
                    Uri.EscapeDataString(RedirectUri),
                    Uri.EscapeDataString("user,repo,notifications,gist"));
            }
        }

        public bool IsEnterprise { get; set; }

        private GitHubAccount _attemptedAccount;
        public GitHubAccount AttemptedAccount
        {
            get { return _attemptedAccount; }
            set { this.RaiseAndSetIfChanged(ref _attemptedAccount, value); }
        }

        public string WebDomain { get; set; }

        public IReactiveCommand<object> GoToOldLoginWaysCommand { get; private set; }

        public IReactiveCommand LoginCommand { get; private set; }

        private string _code;
        public string Code
        {
            get { return _code; }
            set { this.RaiseAndSetIfChanged(ref _code, value); }
        }

        public LoginViewModel(ILoginService loginFactory, 
                              IAccountsService accountsService)
        {
            _loginFactory = loginFactory;

            WebDomain = "https://github.com";

            GoToOldLoginWaysCommand = ReactiveCommand.Create();
            GoToOldLoginWaysCommand.Subscribe(_ => ShowViewModel(CreateViewModel<AddAccountViewModel>()));

            var loginCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Code).Select(x => !string.IsNullOrEmpty(x)), _ => Login(Code));
            loginCommand.Subscribe(x => accountsService.ActiveAccount = x);
            loginCommand.Subscribe(x => MessageBus.Current.SendMessage(new LogoutMessage()));
            LoginCommand = loginCommand;
        }

        private async Task<GitHubAccount> Login(string code)
        {
            string apiUrl;
            if (IsEnterprise)
            {
                apiUrl = WebDomain;
                if (!apiUrl.StartsWith("http://") && !apiUrl.StartsWith("https://"))
                    apiUrl = "https://" + apiUrl;
                if (!apiUrl.EndsWith("/"))
                    apiUrl += "/";
                if (!apiUrl.Contains("/api/"))
                    apiUrl += "api/v3/";

                apiUrl = apiUrl.TrimEnd('/');
            }
            else
            {
                apiUrl = GitHubSharp.Client.DefaultApi;
            }

            var account = AttemptedAccount;
            var loginData = await _loginFactory.LoginWithToken(ClientId, ClientSecret, code, RedirectUri, WebDomain, apiUrl, account);
            return loginData.Account;
        }
    }
}

