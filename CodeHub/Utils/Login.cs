using System;
using MonoTouch.UIKit;
using CodeFramework.Controllers;
using MonoTouch;
using CodeFramework.Utils;
using CodeHub.Data;
using GitHubSharp;
using System.Threading.Tasks;

namespace CodeHub.Utils
{
    public class Login
    {
        public static void LoginWithToken(string accessToken)
        {
            var client = GitHubSharp.Client.BasicOAuth(accessToken);
            var info = client.Execute(client.AuthenticatedUser.GetInfo());
            var username = info.Data.Login;

            GitHubAccount account;
            bool exists;

            //Does this user exist?
            account = Application.Accounts.Find(username);
            exists = account != null;
            if (!exists)
                account = new GitHubAccount { Username = username };
            account.OAuth = accessToken;
            account.AvatarUrl = info.Data.AvatarUrl;

            if (exists)
                Application.Accounts.Update(account);
            else
                Application.Accounts.Insert(account);

            Application.SetUser(account, client);
            new MonoTouch.Foundation.NSObject().InvokeOnMainThread(TransitionToSlideout);
        }

        public static async Task LoginAccount(GitHubAccount account, UIViewController ctrl)
        {
            bool exists = Application.Accounts.Find(account.Username) != null;

            //If it does not exist, or there is no oauth then we need to request it!
            if (exists == false || string.IsNullOrEmpty(account.OAuth))
            {
                await Authenticate(account.Domain, account.Username, account.Password, null, ctrl);
                return;
            }

            //Create the client
            var client = GitHubSharp.Client.BasicOAuth(account.OAuth, account.Domain ?? GitHubSharp.Client.DefaultApi);
            await ctrl.DoWorkAsync("Logging in...", () => {
                var userInfo = client.Execute(client.AuthenticatedUser.GetInfo()).Data;
                account.Username = userInfo.Login;
                account.AvatarUrl = userInfo.AvatarUrl;
            });

            Application.Accounts.Update(account);
            Application.SetUser(account, client);
            TransitionToSlideout();
        }

        public static async Task Authenticate(string domain, string user, string pass, string twoFactor, UIViewController ctrl)
        {
            //Fill these variables in during the proceeding try/catch
            GitHubAccount account;
            bool exists;
            string apiUrl = domain;

            try
            {
                //Make some valid checks
                if (string.IsNullOrEmpty(user))
                    throw new ArgumentException("Username is invalid".t());
                if (string.IsNullOrEmpty(pass))
                    throw new ArgumentException("Password is invalid".t());
                if (apiUrl != null && !Uri.IsWellFormedUriString(apiUrl, UriKind.Absolute))
                    throw new ArgumentException("Domain is invalid".t());

                //Does this user exist?
                account = Application.Accounts.Find(user);
                exists = account != null;
                if (!exists)
                    account = new GitHubAccount { Username = user };

                account.Domain = apiUrl;
                var client = twoFactor == null ? GitHubSharp.Client.Basic(user, pass, apiUrl) : GitHubSharp.Client.BasicTwoFactorAuthentication(user, pass, twoFactor, apiUrl);

                await ctrl.DoWorkAsync("Authenticating...", () => {
                    var scopes = new [] { "user", "public_repo", "repo" , "notifications" , "gist" };
                    var auth = client.Execute(client.Authorizations.GetOrCreate("72f4fb74bdba774b759d", "9253ab615f8c00738fff5d1c665ca81e581875cb", new System.Collections.Generic.List<string>(scopes), "CodeHub", null));
                    account.OAuth = auth.Data.Token;
                });

                if (exists)
                    Application.Accounts.Update(account);
                else
                    Application.Accounts.Insert(account);

                //Create a new client to be used that uses oAuth
                await LoginAccount(account, ctrl);
            }
            catch (StatusCodeException ex)
            {
                //Looks like we need to ask for the key!
                if (ex.Headers.ContainsKey("X-GitHub-OTP"))
                {
                    HandleTwoFactorAuth(apiUrl, user, pass, ctrl);
                    return;
                }


                throw new Exception("Unable to login as user " + user + ". Please check your credentials and try again.");
            }
        }

        /// <summary>
        /// Handle the two factor authentication nonsense by showing an alert view with a textbox for the code
        /// </summary>
        private static void HandleTwoFactorAuth(string apiUrl, string user, string pass, UIViewController ctrl)
        {
            var alert = new UIAlertView();
            alert.Title = "Two-Factor Authentication";
            alert.CancelButtonIndex = alert.AddButton("Cancel");
            var okIndex = alert.AddButton("Ok");
            alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
            alert.Dismissed += async (object sender, UIButtonEventArgs e) => { 
                if (e.ButtonIndex == okIndex)
                {
                    try
                    {
                        await Authenticate(apiUrl, user, pass, alert.GetTextField(0).Text, ctrl);
                    }
                    catch (Exception ex)
                    {
                        MonoTouch.Utilities.ShowAlert("Error".t(), ex.Message);
                    }
                }
            };
            alert.Show();
        }

        private static void TransitionToSlideout()
        {
            var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
            var controller = new ViewControllers.SlideoutNavigationViewController();
            if (appDelegate != null)
                appDelegate.Slideout = controller;
            Transitions.Transition(controller, UIViewAnimationOptions.TransitionFlipFromRight);
        }
    }
}

