using System;
using CodeHub.Core.ViewModels;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using System.Threading.Tasks;
using CodeHub.Core.Factories;
using System.Linq;
using MvvmCross.Platform;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class LoginViewModel : BaseViewModel
    {
        public const string ClientId = "72f4fb74bdba774b759d";
        public const string ClientSecret = "9253ab615f8c00738fff5d1c665ca81e581875cb";
        public static readonly string RedirectUri = "http://dillonbuchanan.com/";
        private readonly ILoginFactory _loginFactory;
        private readonly IFeaturesService _featuresService;

        private bool _isLoggingIn;
        public bool IsLoggingIn
        {
            get { return _isLoggingIn; }
            set
            {
                _isLoggingIn = value;
                RaisePropertyChanged(() => IsLoggingIn);
            }
        }

        public string LoginUrl
        {
            get
            {
                var web = WebDomain.TrimEnd('/');
                return string.Format(
                    web + "/login/oauth/authorize?client_id={0}&redirect_uri={1}&scope={2}", 
                    LoginViewModel.ClientId, 
                    Uri.EscapeDataString(LoginViewModel.RedirectUri),
                    Uri.EscapeDataString("user,repo,notifications,gist"));
            }
        }

        public bool IsEnterprise { get; private set; }

        public GitHubAccount AttemptedAccount { get; private set; }

        public string WebDomain { get; set; }

        public ICommand GoBackCommand
        {
            get { return new MvxCommand(() => ChangePresentation(new MvxClosePresentationHint(this))); }
        }

        public LoginViewModel(ILoginFactory loginFactory, IFeaturesService featuresService)
        {
            _loginFactory = loginFactory;
            _featuresService = featuresService;
        }

        public void Init(NavObject navObject)
        {
            IsEnterprise = navObject.IsEnterprise;
            WebDomain = navObject.WebDomain;

            if (WebDomain == null && !IsEnterprise)
            {
                WebDomain = GitHubSharp.Client.AccessTokenUri;
            }

            if (navObject.AttemptedAccountId >= 0)
            {
                AttemptedAccount = this.GetApplication().Accounts.Find(navObject.AttemptedAccountId) as GitHubAccount;

                //This is a hack to get around the fact that WebDomain will be null for Enterprise users since the last version did not contain the variable
                if (WebDomain == null && IsEnterprise)
                {
                    try
                    {
                        WebDomain = AttemptedAccount.Domain.Substring(0, AttemptedAccount.Domain.IndexOf("/api"));
                    }
                    catch 
                    {
                        //Doh!
                    }
                }
            }
        }

        public async Task Login(string code)
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
    
            LoginData loginData = null;
            bool shouldPromptPush = false;

            try
            {
                IsLoggingIn = true;
                var account = AttemptedAccount;
                loginData = await _loginFactory.LoginWithToken(ClientId, ClientSecret, code, RedirectUri, WebDomain, apiUrl, account);

                if (!_featuresService.IsPushNotificationsActivated && !IsEnterprise)
                {
                    try
                    {
                        var ids = await _featuresService.GetAvailableFeatureIds();
                        shouldPromptPush = ids.Contains(FeatureIds.PushNotifications);
                    }
                    catch
                    {
                    }
                }
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

            try
            {
                // Only prompt if we're allowing that to be enabled.
                if (shouldPromptPush)
                    await Mvx.Resolve<IFeatureFactory>().PromptPushNotificationFeature();
            }
            catch
            {
                // Don't do anything...
            }

            this.GetApplication().ActivateUser(loginData.Account, loginData.Client);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public bool IsEnterprise { get; set; }
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
                    IsEnterprise = !string.Equals(account.Domain, GitHubSharp.Client.DefaultApi), 
                    Username = account.Username,
                    AttemptedAccountId = account.Id
                };
            }
        }
    }
}

