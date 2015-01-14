using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.ViewModels.Accounts;
using MonoTouch.UIKit;
using System.Text;
using ReactiveUI;
using CodeHub.Core.Factories;
using CodeHub.Core.Services;

namespace CodeHub.iOS.Views.Accounts
{
    public class LoginView : BaseWebView<LoginViewModel>
    {
        private readonly IAlertDialogFactory _alertDialogService;

        public LoginView(IAlertDialogFactory alertDialogService, INetworkActivityService networkActivityService)
            : base(networkActivityService)
        {
            _alertDialogService = alertDialogService;

            this.WhenAnyValue(x => x.ViewModel.ShowLoginOptionsCommand).Subscribe(x =>
                NavigationItem.RightBarButtonItem = x == null ? null : x.ToBarButtonItem(UIBarButtonSystemItem.Action));
            this.WhenAnyValue(x => x.ViewModel.LoginUrl).IsNotNull().Subscribe(LoadRequest);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ViewModel.ShowLoginOptionsCommand.CanExecuteObservable.Subscribe(x =>
                Console.WriteLine("Can execute: " + x));
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

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            ViewModel.LoadCommand.ExecuteIfCan();
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
                alert.ContinueWith(t => ViewModel.LoadCommand.ExecuteIfCan(), TaskContinuationOptions.OnlyOnRanToCompletion);
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

        private void LoadRequest(string loginUrl)
        {
            try
            {
                //Remove all cookies & cache
                foreach (var c in MonoTouch.Foundation.NSHttpCookieStorage.SharedStorage.Cookies)
                    MonoTouch.Foundation.NSHttpCookieStorage.SharedStorage.DeleteCookie(c);
                MonoTouch.Foundation.NSUrlCache.SharedCache.RemoveAllCachedResponses();
    			Web.LoadRequest(new MonoTouch.Foundation.NSUrlRequest(new MonoTouch.Foundation.NSUrl(loginUrl)));
            }
            catch (Exception e)
            {
                _alertDialogService.Alert("Unable to process request!", e.Message);
            }
        }
    }
}

