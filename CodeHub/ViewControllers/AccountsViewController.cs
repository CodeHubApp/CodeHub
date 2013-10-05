using MonoTouch.UIKit;
using CodeHub.Data;
using CodeFramework.Controllers;
using CodeFramework.Data;
using System.Collections.Generic;
using System;

namespace CodeHub.ViewControllers
{
	/// <summary>
	/// A list of the accounts that are currently listed with the application
	/// </summary>
    public class AccountsViewController : BaseAccountsViewController
	{
        protected override void AddAccountClicked()
        {
            NavigationController.PushViewController(new AccountTypeViewController(), true);
        }

        protected override async void AccountSelected(Account account)
        {
            var a = account as GitHubAccount;
            //If the account doesn't remember the password we need to prompt
            if (a.DontRemember)
            {
                NavigationController.PushViewController(new LoginViewController(a), true);
            }
            //Change the user!
            else
            {
                try
                {
                    await Utils.Login.LoginAccount(a, this);
                }
                catch (Exception e)
                {
                    MonoTouch.Utilities.ShowAlert("Error", e.Message);
                    NavigationController.PushViewController(new LoginViewController(a), true);
                }
            }
        }
    }
}

