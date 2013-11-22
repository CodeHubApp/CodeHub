using System;
using CodeHub.Core.Data;
using CodeFramework.iOS.Views;

namespace CodeHub.iOS.Views.Accounts
{
    public class WebLoginViewController : WebView
    {
        private const string ClientId = "72f4fb74bdba774b759d";
        private const string ClientSecret = "9253ab615f8c00738fff5d1c665ca81e581875cb";

        private GitHubAccount _attemptedAccount;

        public WebLoginViewController(GitHubAccount attemptedAccount)
            : base(false)
        {
            Title = "Login";

            _attemptedAccount = attemptedAccount;

            LoadRequest();
        }


        public WebLoginViewController()
            : base(false)
        {
            Title = "Login";

            LoadRequest();
        }

        protected override bool ShouldStartLoad(MonoTouch.Foundation.NSUrlRequest request, MonoTouch.UIKit.UIWebViewNavigationType navigationType)
        {
            Console.WriteLine("Attemping to load: " + request.Url);

            //We're being redirected to our redirect URL so we must have been successful
            if (request.Url.Host == "dillonbuchanan.com")
            {
                var code = request.Url.Query.Split('=')[1];

//                this.DoWorkNoHud(() => {
//                    var token = GitHubSharp.Client.RequestAccessToken(ClientId, ClientSecret, code, null);
//                    CodeHub.Utils.Login.LoginWithToken(token.AccessToken);
//                }, ex => {
//                    MonoTouch.Utilities.LogException("Unable to access token", ex);
//                    MonoTouch.Utilities.ShowAlert("Unable to Login", "Looks like something has gone wrong. Please try again.", LoadRequest);
//                });

                return false;
            }
            return base.ShouldStartLoad(request, navigationType);
        }

        protected override void OnLoadFinished(object sender, EventArgs e)
        {
            base.OnLoadFinished(sender, e);

            //Inject some Javascript so we can set the username if there is an attempted account
            if (_attemptedAccount != null)
            {
                var script = "(function() { setTimeout(function() { $('input[name=\"login\"]').val('" + _attemptedAccount.Username + "').attr('readonly', 'readonly'); }, 100); })();";
                Web.EvaluateJavascript(script);
            }
        }

        private void LoadRequest()
        {
            //Remove all cookies & cache
            foreach (var c in MonoTouch.Foundation.NSHttpCookieStorage.SharedStorage.Cookies)
                MonoTouch.Foundation.NSHttpCookieStorage.SharedStorage.DeleteCookie(c);
            MonoTouch.Foundation.NSUrlCache.SharedCache.RemoveAllCachedResponses();


            var url = string.Format("https://github.com/login/oauth/authorize?client_id={0}&redirect_uri={1}&scope={2}", 
                                    ClientId, Uri.EscapeUriString("http://dillonbuchanan.com/"), Uri.EscapeUriString("user,public_repo,repo,notifications,gist"));
            Web.LoadRequest(new MonoTouch.Foundation.NSUrlRequest(new MonoTouch.Foundation.NSUrl(url)));
        }
    }
}

