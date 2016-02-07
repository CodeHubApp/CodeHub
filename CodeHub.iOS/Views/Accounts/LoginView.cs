using System;
using CodeHub.iOS.Views;
using CodeHub.Core.ViewModels.Accounts;
using UIKit;
using MvvmCross.Platform;
using CodeFramework.iOS.Utils;
using Foundation;
using WebKit;
using CodeHub.Core.Services;

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

            if (!ViewModel.IsEnterprise || ViewModel.AttemptedAccount != null)
				LoadRequest();

            var hud = this.CreateHud();

            ViewModel.Bind(x => x.IsLoggingIn, x =>
            {
                if (x)
                    hud.Show("Logging in...");
                else
                    hud.Hide();
            });
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);

			if (ViewModel.IsEnterprise && string.IsNullOrEmpty(ViewModel.WebDomain))
			{
				Stuff();
			}
		}

		private void Stuff()
		{
			var alert = new UIAlertView();
			alert.Title = "Enterprise URL";
			alert.Message = "Please enter the webpage address for the GitHub Enterprise installation";
			alert.CancelButtonIndex = alert.AddButton("Cancel");
			var okButton = alert.AddButton("Ok");
			alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
			alert.Clicked += (sender, e) => {
				if (e.ButtonIndex == okButton)
				{
					var txt = alert.GetTextField(0);
					ViewModel.WebDomain = txt.Text;
					LoadRequest();
				}
				if (e.ButtonIndex == alert.CancelButtonIndex)
				{
					ViewModel.GoBackCommand.Execute(null);
				}
			};

			alert.Show();
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
            if (e.Code == 102 || e.Code == -999)
				return;

			if (ViewModel.IsEnterprise)
				MonoTouch.Utilities.ShowAlert("Error", "Unable to communicate with GitHub with given Url. " + e.LocalizedDescription, Stuff);
			else
				MonoTouch.Utilities.ShowAlert("Error", "Unable to communicate with GitHub. " + e.LocalizedDescription);
		}

        private void LoadRequest()
        {
            try
            {
                //Remove all cookies & cache
                foreach (var c in Foundation.NSHttpCookieStorage.SharedStorage.Cookies)
                    Foundation.NSHttpCookieStorage.SharedStorage.DeleteCookie(c);
                Foundation.NSUrlCache.SharedCache.RemoveAllCachedResponses();
    			Web.LoadRequest(new Foundation.NSUrlRequest(new Foundation.NSUrl(ViewModel.LoginUrl)));
            }
            catch (Exception e)
            {
                Mvx.Resolve<IAlertDialogService>().Alert("Unable to process request!", e.Message);
            }
        }
    }
}

