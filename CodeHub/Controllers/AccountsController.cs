using System;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using CodeHub.Data;
using CodeFramework.Elements;
using CodeFramework.Controllers;
using CodeFramework.Views;
using System.Linq;
using System.Collections.Generic;

namespace CodeHub.Controllers
{
	/// <summary>
	/// A list of the accounts that are currently listed with the application
	/// </summary>
	public class AccountsController : BaseAccountsController
	{
        protected override void AccountDeleted(CodeFramework.Data.IAccount account)
        {
            var thisAccount = (Account)account;
            Application.Accounts.Remove(thisAccount);

            if (Application.Account != null && Application.Account.Equals(thisAccount))
            {
                NavigationItem.LeftBarButtonItem.Enabled = false;
                Application.Account = null;
            }
        }

        protected override List<AccountElement> PopulateAccounts()
        {
            var accountElements = new List<AccountElement>();
            foreach (var account in Application.Accounts)
            {
                var thisAccount = account;
                var t = new AccountElement(thisAccount);
                t.Tapped += () => { 
                    //If the account doesn't remember the password we need to prompt
                    if (thisAccount.DontRemember)
                    {
                        var loginController = new CodeHub.GitHub.Controllers.Accounts.LoginViewController() { Username = thisAccount.Username };
                        loginController.Login = (username, password) => {
                            Utils.Login.LoginAccount(username, password, loginController);
                        };
                        NavigationController.PushViewController(loginController, true);
                    }
                    //Change the user!
                    else
                    {
                        Utils.Login.LoginAccount(thisAccount.Username, thisAccount.Password, this);
                    }
                };

                //Check to see if this account is the active account. Application.Account could be null 
                //so make it the target of the equals, not the source.
                if (thisAccount.Equals(Application.Accounts.ActiveAccount))
                    t.Accessory = UITableViewCellAccessory.Checkmark;
                accountElements.Add(t);
            }

            return accountElements;
        }

        protected override void AddAccountClicked()
        {
            var ctrl = new CodeHub.GitHub.Controllers.Accounts.LoginViewController();
            ctrl.Login = (username, password) => {
                Utils.Login.LoginAccount(username, password, ctrl);
            };
            NavigationController.PushViewController(ctrl, true);
        }
    }
}

