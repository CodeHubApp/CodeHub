using System;
using MonoTouch.UIKit;
using CodeFramework.Controllers;
using MonoTouch;
using CodeFramework.Utils;
using CodeHub.Data;

namespace CodeHub.Utils
{
    public class Login
    {
        public static void LoginAccount(string domain, string user, string pass, UIViewController ctrl, Action<Exception> error = null)
        {
            //Does this user exist?
            var account = Application.Accounts.Find(user);
            var exists = account != null;
            if (!exists)
                account = new Account { Username = user, Password = pass };

            ctrl.DoWork("Logging in...", () => {

                var client = new GitHubSharp.Client(user, pass) { Timeout = 30 * 1000 };
                var userInfo = client.AuthenticatedUser.GetInfo().Data;

                account.Domain = domain;
                account.Username = userInfo.Login;
                account.AvatarUrl = userInfo.AvatarUrl;
                account.Organizations = client.AuthenticatedUser.GetOrganizations().Data;

                if (exists)
                    Application.Accounts.Update(account);
                else
                    Application.Accounts.Insert(account);

                Application.SetUser(account, client);
                ctrl.InvokeOnMainThread(TransitionToSlideout);

            }, ex => {
                //If there is a login failure, unset the user
                Application.UnsetUser();
                Utilities.ShowAlert("Unable to Authenticate", "Unable to login as user " + account.Username + ". Please check your credentials and try again. Remember, credentials are case sensitive!");
                if (error != null)
                    error(ex);
            });
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

