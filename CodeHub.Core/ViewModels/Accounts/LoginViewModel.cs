using System;
using CodeHub.Core.ViewModels;
using CodeHub.Core.Data;
using System.Threading.Tasks;
using CodeHub.Core.Factories;
using ReactiveUI;
using CodeHub.Core.Messages;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class LoginViewModel : BaseViewModel
    {
        public static readonly string RedirectUri = "http://dillonbuchanan.com/";
        private readonly ILoginFactory _loginFactory;

        private bool _isLoggingIn;
        public bool IsLoggingIn
        {
            get { return _isLoggingIn; }
            set { this.RaiseAndSetIfChanged(ref _isLoggingIn, value); }
        }

        public string LoginUrl
        {
            get
            {
                var web = WebDomain.TrimEnd('/');
                return string.Format(
                    web + "/login/oauth/authorize?client_id={0}&redirect_uri={1}&scope={2}", 
                    Secrets.GithubOAuthId, 
                    Uri.EscapeDataString(LoginViewModel.RedirectUri),
                    Uri.EscapeDataString("user:follow,repo,notifications,gist,read:org"));
            }
        }

        public GitHubAccount AttemptedAccount { get; private set; }

        public string WebDomain { get; set; }

        public LoginViewModel(ILoginFactory loginFactory)
        {
            _loginFactory = loginFactory;
            Title = "Login";
        }

        public void Init(NavObject navObject)
        {
            WebDomain = navObject.WebDomain ?? GitHubSharp.Client.AccessTokenUri;

            if (navObject.AttemptedAccountId >= 0)
            {
                AttemptedAccount = this.GetApplication().Accounts.Find(navObject.AttemptedAccountId);
            }
        }

        public async Task Login(string code)
        {
            LoginData loginData = null;

            try
            {
                IsLoggingIn = true;
                loginData = await _loginFactory.LoginWithToken(Secrets.GithubOAuthId, Secrets.GithubOAuthSecret, 
                    code, RedirectUri, WebDomain, GitHubSharp.Client.DefaultApi);
            }
            catch (Exception e)
            {
                DisplayAlert(e.Message);
                return;
            }
            finally
            {
                IsLoggingIn = false;
            }

            this.GetApplication().ActivateUser(loginData.Account, loginData.Client);
            MessageBus.Current.SendMessage(new LogoutMessage());
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string WebDomain { get; set; }
            public int AttemptedAccountId { get; set; }

            public NavObject()
            {
                AttemptedAccountId = int.MinValue;
            }

            public static NavObject CreateDontRemember(GitHubAccount account)
            {
                return new NavObject
                { 
                    WebDomain = account.WebDomain, 
                    Username = account.Username,
                    AttemptedAccountId = account.Id
                };
            }
        }
    }
}

