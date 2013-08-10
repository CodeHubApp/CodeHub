using System;
using MonoTouch.UIKit;
using System.Linq;
using CodeFramework.Controllers;
using CodeHub.Data;
using CodeHub.GitHub.Controllers.Accounts;
using CodeFramework.Utils;

namespace CodeHub.Controllers
{
    public class StartupController : CodeFramework.Controllers.StartupController
    {
        /// <summary>
        /// Processes the accounts.
        /// </summary>
        protected override void ProcessAccounts()
        {
            var defaultAccount = GetDefaultAccount();

            //There's no accounts... or something bad has happened to the default
            if (Application.Accounts.Count == 0 || defaultAccount == null)
            {
                var login = new LoginViewController();
                login.NavigationItem.LeftBarButtonItem = null;
                login.Login = (username, password) => {
                    Utils.Login.LoginAccount(username, password, login);
                };

                var navCtrl = new CustomNavigationController(this, login);
                Transitions.TransitionToController(navCtrl);
                return;
            }

            //Don't remember, prompt for password
            if (defaultAccount.DontRemember)
            {
                var accountsController = new AccountsController();
                accountsController.NavigationItem.LeftBarButtonItem = null;
                var login = new LoginViewController { Username = defaultAccount.Username };
                login.Login = (username, password) => {
                    Utils.Login.LoginAccount(username, password, login);
                };

                var navigationController = new CustomNavigationController(this, accountsController);
                navigationController.PushViewController(login, false);
                Transitions.TransitionToController(navigationController);
            }
            //If the user wanted to remember the account
            else
            {
                Utils.Login.LoginAccount(defaultAccount.Username, defaultAccount.Password, this);
            }
        }

        /// <summary>
        /// Gets the default account. If there is not one assigned it will pick the first in the account list.
        /// If there isn't one, it'll just return null.
        /// </summary>
        /// <returns>The default account.</returns>
        private Account GetDefaultAccount()
        {
            var defaultAccount = Application.Accounts.GetDefault();
            if (defaultAccount == null)
            {
                defaultAccount = Application.Accounts.FirstOrDefault();
                Application.Accounts.SetDefault(defaultAccount);
            }
            return defaultAccount;
        }
    }
}

