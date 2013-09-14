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
        public async static Task LoginAccount(string domain, string user, string pass, UIViewController ctrl)
        {
            //Fill these variables in during the proceeding try/catch
            Account account;
            bool exists;

            try
            {
                //Make some valid checks
                if (string.IsNullOrEmpty(user))
                    throw new ArgumentException("Username is invalid".t());
                if (string.IsNullOrEmpty(pass))
                    throw new ArgumentException("Password is invalid".t());
                if (domain != null && !Uri.IsWellFormedUriString(domain, UriKind.Absolute))
                    throw new ArgumentException("Domain is invalid".t());

                //Does this user exist?
                account = Application.Accounts.Find(user);
                exists = account != null;
                if (!exists)
                    account = new Account { Username = user, Password = pass };

                var client = (domain == null) ? new GitHubSharp.Client(user, pass) : new GitHubSharp.Client(user, pass, domain);
                client.Timeout = 30 * 1000;

                await ctrl.DoWorkAsync("Logging in...", () => {
                    var userInfo = client.AuthenticatedUser.GetInfo().Data;
                    account.Domain = domain;
                    account.Username = userInfo.Login;
                    account.AvatarUrl = userInfo.AvatarUrl;
                });

                if (exists)
                    Application.Accounts.Update(account);
                else
                    Application.Accounts.Insert(account);

                Application.SetUser(account, client);
                TransitionToSlideout();
            }
            catch (StatusCodeException)
            {
                throw new Exception("Unable to login as user " + user + ". Please check your credentials and try again.");
            }
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

