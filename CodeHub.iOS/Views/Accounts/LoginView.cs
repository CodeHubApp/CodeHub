using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Accounts;
using UIKit;
using System.Text;
using ReactiveUI;
using CodeHub.Core.Factories;
using CodeHub.Core.Services;
using CodeHub.iOS.ViewComponents;

namespace CodeHub.iOS.Views.Accounts
{
    public class LoginView : BaseWebView<OAuthFlowLoginViewModel>
    {
        private readonly IAlertDialogFactory _alertDialogService;
        private static readonly string HasSeenWelcomeKey = "HAS_SEEN_WELCOME";

        private static readonly string OAuthWelcome = 
            "In the following screen you will be prompted for your GitHub credentials. This is done through GitHub's OAuth portal, " +
            "the recommended way to authenticate.\n\nCodeHub does not save your password. Instead, only the OAuth " + 
            "token is saved on the device which you may revoke at any time.";

        public LoginView(IAlertDialogFactory alertDialogService, IDefaultValueService defaultValueService)
        {
            _alertDialogService = alertDialogService;

            this.WhenAnyValue(x => x.ViewModel.ShowLoginOptionsCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Action))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            this.WhenAnyValue(x => x.ViewModel.LoginUrl)
                .IsNotNull()
                .Subscribe(LoadRequest);

            bool hasSeenWelcome;
            if (!defaultValueService.TryGet(HasSeenWelcomeKey, out hasSeenWelcome))
                hasSeenWelcome = false;

            this.Appeared
                .Where(_ => !hasSeenWelcome)
                .Take(1)
                .Subscribe(_ => 
                    BlurredAlertView.Display(OAuthWelcome, () => defaultValueService.Set(HasSeenWelcomeKey, true)));
        }

		protected override bool ShouldStartLoad(Foundation.NSUrlRequest request, UIWebViewNavigationType navigationType)
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

            _alertDialogService.Alert("Error", "Unable to communicate with GitHub. " + e.Error.LocalizedDescription).ToBackground();
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
            ViewModel.AttemptedAccount.Do(x => 
                script.Append("$('input[name=\"login\"]').val('" + x.Username + "').attr('readonly', 'readonly');"));

            Web.EvaluateJavascript("(function(){setTimeout(function(){" + script +"}, 100); })();");
        }

        private void LoadRequest(string loginUrl)
        {
            try
            {
                //Remove all cookies & cache
                foreach (var c in Foundation.NSHttpCookieStorage.SharedStorage.Cookies)
                    Foundation.NSHttpCookieStorage.SharedStorage.DeleteCookie(c);
                Foundation.NSUrlCache.SharedCache.RemoveAllCachedResponses();
    			Web.LoadRequest(new Foundation.NSUrlRequest(new Foundation.NSUrl(loginUrl)));
            }
            catch (Exception e)
            {
                _alertDialogService.Alert("Unable to process request!", e.Message);
            }
        }
    }
}

