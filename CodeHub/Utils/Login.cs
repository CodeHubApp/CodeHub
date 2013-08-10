using System;
using MonoTouch.UIKit;
using CodeFramework.Controllers;
using System.Linq;
using MonoTouch;
using CodeFramework.Utils;
using CodeHub.Data;

namespace CodeHub.Utils
{
    public class Login
    {
        public static void LoginAccount(string user, string pass, UIViewController ctrl, Action error = null)
        {
            //Does this user exist?
            var account = Application.Accounts.Find(user);
            var exists = account != null;
            if (!exists)
                account = new Account { Username = user, Password = pass };

            ctrl.DoWork("Logging in...", () => {

                var client = new GitHubSharp.Client(user, pass) { Timeout = 30 * 1000 };
                var userInfo = client.API.GetAuthenticatedUser().Data;

                account.FullName = userInfo.Name;
                account.Username = userInfo.Login;
                account.AvatarUrl = userInfo.AvatarUrl;
                account.Organizations = client.API.GetOrganizations().Data;

                if (exists)
                    Application.Accounts.Update(account);
                else
                    Application.Accounts.Insert(account);

                Application.SetUser(account);
                ctrl.InvokeOnMainThread(TransitionToSlideout);

            }, (ex) => {
                //If there is a login failure, unset the user
                Application.SetUser(null);
                Utilities.ShowAlert("Unable to Authenticate", "Unable to login as user " + account.Username + ". Please check your credentials and try again. Remember, credentials are case sensitive!");
                if (error != null)
                    error();
            });
        }

        private static void TransitionToSlideout()
        {
            var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
            var controller = new CodeHub.Controllers.SlideoutNavigationController();
            if (appDelegate != null)
                appDelegate.Slideout = controller;
            Transitions.Transition(controller, UIViewAnimationOptions.TransitionFlipFromRight);
        }
    }
}

