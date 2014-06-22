using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Accounts;
using MonoTouch.UIKit;
using System.Text;
using ReactiveUI;
using Xamarin.Utilities.Core.Services;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Accounts
{
    public class LoginView : WebView<LoginViewModel>
    {
        private readonly IAlertDialogService _alertDialogService;
        private readonly IStatusIndicatorService _statusIndicatorService;
        private UIActionSheet _actionSheet;

        public LoginView(IAlertDialogService alertDialogService, IStatusIndicatorService statusIndicatorService)
        {
            _alertDialogService = alertDialogService;
            _statusIndicatorService = statusIndicatorService;
        }

		private void ShowExtraMenu()
		{
            _actionSheet = new UIActionSheet("Login");
            var basicButton = _actionSheet.AddButton("Login via BASIC");
            var cancelButton = _actionSheet.AddButton("Cancel");
            _actionSheet.CancelButtonIndex = cancelButton;
            _actionSheet.DismissWithClickedButtonIndex(cancelButton, true);
            _actionSheet.Clicked += (s, e) =>
            {
				if (e.ButtonIndex == basicButton)
					ViewModel.GoToOldLoginWaysCommand.ExecuteIfCan();
                _actionSheet = null;
            };

			_actionSheet.ShowInView(View);
		}

		public override void ViewDidLoad()
		{
            Title = "Login";
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu());

			base.ViewDidLoad();

			if (!ViewModel.IsEnterprise || ViewModel.AttemptedAccount != null)
				LoadRequest();

            ViewModel.LoginCommand.IsExecuting.Skip(1).Subscribe(x => 
            {
                if (x)
                    _statusIndicatorService.Show("Logging in...");
                else
                    _statusIndicatorService.Hide();
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

		private async Task Stuff()
		{
		    try
		    {
                var response = await _alertDialogService.PromptTextBox("Enterprise URL",
                                      "Please enter the webpage address for the GitHub Enterprise installation", 
                                      ViewModel.WebDomain, "Ok");
                ViewModel.WebDomain = response;
                LoadRequest();
		    }
		    catch (Exception)
		    {
		        ViewModel.DismissCommand.ExecuteIfCan();
		    }
		}

		protected override bool ShouldStartLoad(MonoTouch.Foundation.NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            //We're being redirected to our redirect URL so we must have been successful
            if (request.Url.Host == "dillonbuchanan.com")
            {
                ViewModel.Code = request.Url.Query.Split('=')[1];
				ViewModel.LoginCommand.ExecuteIfCan();
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
            {
                var alert = _alertDialogService.Alert("Error", "Unable to communicate with GitHub with given Url. " + e.Error.LocalizedDescription);
                alert.ContinueWith(t => Stuff(), TaskContinuationOptions.OnlyOnRanToCompletion);
            }
			else
                _alertDialogService.Alert("Error", "Unable to communicate with GitHub. " + e.Error.LocalizedDescription);
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

            Web.EvaluateJavascript("(function(){setTimeout(function(){" + script +"}, 100); })();");
        }

        private void LoadRequest()
        {
            try
            {
                //Remove all cookies & cache
                foreach (var c in MonoTouch.Foundation.NSHttpCookieStorage.SharedStorage.Cookies)
                    MonoTouch.Foundation.NSHttpCookieStorage.SharedStorage.DeleteCookie(c);
                MonoTouch.Foundation.NSUrlCache.SharedCache.RemoveAllCachedResponses();
    			Web.LoadRequest(new MonoTouch.Foundation.NSUrlRequest(new MonoTouch.Foundation.NSUrl(ViewModel.LoginUrl)));
            }
            catch (Exception e)
            {
                _alertDialogService.Alert("Unable to process request!", e.Message);
            }
        }
    }
}

