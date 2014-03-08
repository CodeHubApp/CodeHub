using System;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Accounts;
using MonoTouch.UIKit;
using System.Text;

namespace CodeHub.iOS.Views.Accounts
{
    public class LoginView : WebView
    {
		public new LoginViewModel ViewModel
		{
			get { return (LoginViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}

        public LoginView()
            : base(false)
        {
            Title = "Login";
			NavigationItem.RightBarButtonItem = new MonoTouch.UIKit.UIBarButtonItem(MonoTouch.UIKit.UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu());
        }

		private void ShowExtraMenu()
		{
			var sheet = MonoTouch.Utilities.GetSheet("Login");
			var basicButton = sheet.AddButton("Login via BASIC");
			var cancelButton = sheet.AddButton("Cancel".t());
			sheet.CancelButtonIndex = cancelButton;
			sheet.DismissWithClickedButtonIndex(cancelButton, true);
			sheet.Clicked += (s, e) => {
				// Pin to menu
				if (e.ButtonIndex == basicButton)
				{
					ViewModel.GoToOldLoginWaysCommand.Execute(null);
				}
			};

			sheet.ShowInView(this.View);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			if (!ViewModel.IsEnterprise || ViewModel.AttemptedAccount != null)
				LoadRequest();
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

		protected override bool ShouldStartLoad(MonoTouch.Foundation.NSUrlRequest request, MonoTouch.UIKit.UIWebViewNavigationType navigationType)
        {
            //We're being redirected to our redirect URL so we must have been successful
            if (request.Url.Host == "dillonbuchanan.com")
            {
                var code = request.Url.Query.Split('=')[1];
				ViewModel.Login(code);
                return false;
            }
            if (request.Url.AbsoluteString == "https://github.com/" || request.Url.AbsoluteString.StartsWith("https://github.com/join"))
            {
                return false;
            }

            return base.ShouldStartLoad(request, navigationType);
        }

		protected override void OnLoadError(object sender, UIWebErrorArgs e)
		{
			base.OnLoadError(sender, e);

			//Frame interrupted error
			if (e.Error.Code == 102)
				return;

			if (ViewModel.IsEnterprise)
				MonoTouch.Utilities.ShowAlert("Error", "Unable to communicate with GitHub with given Url. " + e.Error.LocalizedDescription, Stuff);
			else
				MonoTouch.Utilities.ShowAlert("Error", "Unable to communicate with GitHub. " + e.Error.LocalizedDescription);
		}

        protected override void OnLoadFinished(object sender, EventArgs e)
        {
            base.OnLoadFinished(sender, e);

            var script = new StringBuilder();

            //Apple is full of clowns. The GitHub login page has links that can ultimiately end you at a place where you can purchase something
            //so we need to inject javascript that will remove these links. What a bunch of idiots...
            script.Append("$('.switch-to-desktop').hide();");
            script.Append("$('.header-button').hide();");
            script.Append("$('.header').hide();");
            script.Append("$('.site-footer').hide();");
            script.Append("$('.brand-logo-wordmark').click(function(e) { e.preventDefault(); });");

            //Inject some Javascript so we can set the username if there is an attempted account
			if (ViewModel.AttemptedAccount != null)
            {
                script.Append("$('input[name=\"login\"]').val('" + ViewModel.AttemptedAccount.Username + "').attr('readonly', 'readonly');");
            }

            Web.EvaluateJavascript("(function(){setTimeout(function(){" + script.ToString() +"}, 100); })();");
        }

        private void LoadRequest()
        {
            //Remove all cookies & cache
            foreach (var c in MonoTouch.Foundation.NSHttpCookieStorage.SharedStorage.Cookies)
                MonoTouch.Foundation.NSHttpCookieStorage.SharedStorage.DeleteCookie(c);
            MonoTouch.Foundation.NSUrlCache.SharedCache.RemoveAllCachedResponses();
			Web.LoadRequest(new MonoTouch.Foundation.NSUrlRequest(new MonoTouch.Foundation.NSUrl(ViewModel.LoginUrl)));
        }
    }
}

