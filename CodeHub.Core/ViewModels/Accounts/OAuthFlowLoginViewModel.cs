using System;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.Messages;
using System.Reactive;
using CodeHub.Core.Factories;
using CodeHub.Core.Utilities;
using GitHubSharp;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class OAuthFlowLoginViewModel : BaseViewModel
    {
        public const string ClientId = "72f4fb74bdba774b759d";
        public const string ClientSecret = "9253ab615f8c00738fff5d1c665ca81e581875cb";
        public static readonly string RedirectUri = "http://dillonbuchanan.com/";
        private const string DefaultWebDomain = "https://github.com";
        private readonly IAccountsRepository _accountsRepository;
        private readonly IAlertDialogFactory _alertDialogService;

        private readonly ObservableAsPropertyHelper<string> _loginUrl;
        public string LoginUrl
        {
            get { return _loginUrl.Value; }
        }

        private GitHubAccount _attemptedAccount;
        public GitHubAccount AttemptedAccount
        {
            get { return _attemptedAccount; }
            set { this.RaiseAndSetIfChanged(ref _attemptedAccount, value); }
        }

        private string _webDomain;
        public string WebDomain
        {
            get { return _webDomain; }
            set { this.RaiseAndSetIfChanged(ref _webDomain, value); }
        }

        public IReactiveCommand<GitHubAccount> LoginCommand { get; private set; }

        public IReactiveCommand<Unit> ShowLoginOptionsCommand { get; private set; }

        private string _code;
        public string Code
        {
            get { return _code; }
            set { this.RaiseAndSetIfChanged(ref _code, value); }
        }

        public OAuthFlowLoginViewModel(
            IAccountsRepository accountsRepository,
            IActionMenuFactory actionMenuService,
            IAlertDialogFactory alertDialogService)
        {
            _accountsRepository = accountsRepository;
            _alertDialogService = alertDialogService;

            Title = "Login";

            var oauthLogin = ReactiveCommand.Create().WithSubscription(_ =>
                NavigateTo(this.CreateViewModel<OAuthTokenLoginViewModel>()));

            var canLogin = this.WhenAnyValue(x => x.Code).Select(x => !string.IsNullOrEmpty(x));
            var loginCommand = ReactiveCommand.CreateAsyncTask(canLogin,_ => Login(Code));
            loginCommand.Subscribe(x => MessageBus.Current.SendMessage(new LogoutMessage()));
            LoginCommand = loginCommand;

            ShowLoginOptionsCommand = ReactiveCommand.CreateAsyncTask(sender =>
            {
                var actionMenu = actionMenuService.Create(Title);
                actionMenu.AddButton("Login via Token", oauthLogin);
                return actionMenu.Show(sender);
            });

            _loginUrl = this.WhenAnyValue(x => x.WebDomain)
                .IsNotNull()
                .Select(x => x.TrimEnd('/'))
                .Select(x => 
                    string.Format(x + "/login/oauth/authorize?client_id={0}&redirect_uri={1}&scope={2}", 
                    ClientId, Uri.EscapeDataString(RedirectUri), Uri.EscapeDataString(string.Join(",", OctokitClientFactory.Scopes))))
                .ToProperty(this, x => x.LoginUrl);

            WebDomain = DefaultWebDomain;
        }

        private async Task<GitHubAccount> Login(string code)
        {
            using (_alertDialogService.Activate("Logging in..."))
            {
                var token = await Client.RequestAccessToken(ClientId, ClientSecret, code, RedirectUri, WebDomain);
                var client = Client.BasicOAuth(token.AccessToken);
                var info = (await client.ExecuteAsync(client.AuthenticatedUser.GetInfo())).Data;
                client.Username = info.Login;

                var account = (await _accountsRepository.Find(Client.DefaultApi, info.Login)) ?? new GitHubAccount();
                account.Username = info.Login;
                account.OAuth = token.AccessToken;
                account.AvatarUrl = info.AvatarUrl;
                account.Name = info.Name;
                account.Email = info.Email;
                account.Domain = Client.DefaultApi;
                account.WebDomain = WebDomain;
                account.IsEnterprise = false;

                await _accountsRepository.Insert(account);
                await _accountsRepository.SetDefault(account);
                return account;
            }
        }

    }
}

