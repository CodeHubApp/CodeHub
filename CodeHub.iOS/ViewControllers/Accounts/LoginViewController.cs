using System;
using CodeHub.Core.ViewModels.Accounts;
using MvvmCross.Platform;
using CodeHub.iOS.Utilities;
using Foundation;
using WebKit;
using CodeHub.Core.Services;
using CodeHub.iOS.Services;
using CodeHub.Core.Factories;
using System.Reactive.Linq;
using CodeHub.iOS.Views;
using System.Linq;

namespace CodeHub.iOS.ViewControllers.Accounts
{
    public class LoginViewController : BaseWebViewController
    {
        private static readonly string OAuthWelcome = 
            "In the following screen you will be prompted for your GitHub credentials. This is done through GitHub's OAuth portal, " +
            "the recommended way to authenticate.\n\nCodeHub does not save your password. Instead, only the OAuth " + 
            "token is saved on the device which you may revoke at any time.";
        
        public LoginViewModel ViewModel { get; }

        public LoginViewController() 
            : base(true)
        {
            Title = "Login";
            ViewModel = new LoginViewModel(Mvx.Resolve<ILoginFactory>());
            ViewModel.Init(new LoginViewModel.NavObject());

            OnActivation(d => d(ViewModel.Bind(x => x.IsLoggingIn).SubscribeStatus("Logging in...")));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            LoadRequest();

            if (!Core.Settings.HasSeenOAuthWelcome)
            {
                Appeared
                    .Take(1)
                    .Do(_ => Core.Settings.HasSeenOAuthWelcome = true)
                    .Subscribe(_ => BlurredAlertView.Display(OAuthWelcome));
            }
        }

        protected override bool ShouldStartLoad(WKWebView webView, WKNavigationAction navigationAction)
        {
            try
            {
                //We're being redirected to our redirect URL so we must have been successful
                if (navigationAction.Request.Url.Host == "dillonbuchanan.com")
                {
                    var queryParameters = navigationAction.Request.Url.Query.Split('&');

                    var code = queryParameters.FirstOrDefault(x => x.StartsWith("code=", StringComparison.OrdinalIgnoreCase));
                    var codeValue = code?.Replace("code=", String.Empty);
                    ViewModel.Login(codeValue);
                    return false;
                }
    
                if (navigationAction.Request.Url.AbsoluteString.StartsWith("https://github.com/join"))
                {
                    Mvx.Resolve<IAlertDialogService>().Alert("Error", "Sorry, due to restrictions, creating GitHub accounts cannot be done in CodeHub.");
                    return false;
                }

                return base.ShouldStartLoad(webView, navigationAction);
            }
            catch 
            {
                Mvx.Resolve<IAlertDialogService>().Alert("Error Logging in!", "CodeHub is unable to login you in due to an unexpected error. Please try again.");
                return false;
            }
        }

        protected override void OnLoadError(NSError e)
        {
            base.OnLoadError(e);

            //Frame interrupted error
            if (e.Code == 102 || e.Code == -999) return;
            AlertDialogService.ShowAlert("Error", "Unable to communicate with GitHub. " + e.LocalizedDescription);
        }

        private void LoadRequest()
        {
            //Remove all cookies & cache
            WKWebsiteDataStore.DefaultDataStore.RemoveDataOfTypes(WKWebsiteDataStore.AllWebsiteDataTypes, NSDate.FromTimeIntervalSince1970(0), 
                () => Web.LoadRequest(new NSUrlRequest(new NSUrl(ViewModel.LoginUrl))));
        }
    }
}

