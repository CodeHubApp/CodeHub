using System;
using CodeHub.iOS.Views;
using CodeHub.Core.ViewModels.Accounts;
using MvvmCross.Platform;
using CodeHub.iOS.Utilities;
using Foundation;
using WebKit;
using CodeHub.Core.Services;
using CodeHub.iOS.Services;

namespace CodeHub.iOS.Views.Accounts
{
    public class LoginView : WebView
    {
		public new LoginViewModel ViewModel
		{
			get { return (LoginViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}

        public LoginView() : base(true)
        {
            Title = "Login";
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            var hud = this.CreateHud();
            ViewModel.Bind(x => x.IsLoggingIn, x =>
            {
                if (x)
                    hud.Show("Logging in...");
                else
                    hud.Hide();
            });

			LoadRequest();
		}

        protected override bool ShouldStartLoad(WKWebView webView, WKNavigationAction navigationAction)
        {
			try
			{
	            //We're being redirected to our redirect URL so we must have been successful
                if (navigationAction.Request.Url.Host == "dillonbuchanan.com")
	            {
                    var code = navigationAction.Request.Url.Query.Split('=')[1];
					ViewModel.Login(code);
	                return false;
	            }
    
                if (navigationAction.Request.Url.AbsoluteString.StartsWith("https://github.com/join"))
                {
                    Mvx.Resolve<IAlertDialogService>().Alert("Error", "Sorry, due to Apple restrictions, creating GitHub accounts cannot be done in CodeHub.");
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
            try
            {
                //Remove all cookies & cache
                WKWebsiteDataStore.DefaultDataStore.RemoveDataOfTypes(WKWebsiteDataStore.AllWebsiteDataTypes, NSDate.FromTimeIntervalSince1970(0), () => {
                    Web.LoadRequest(new NSUrlRequest(new Foundation.NSUrl(ViewModel.LoginUrl)));
                });
            }
            catch (Exception e)
            {
                Mvx.Resolve<IAlertDialogService>().Alert("Unable to process request!", e.Message);
            }
        }
    }
}

