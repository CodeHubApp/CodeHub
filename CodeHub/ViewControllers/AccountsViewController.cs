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
    public class AccountsViewController : BaseAccountsController
	{
        protected override List<AccountElement> PopulateAccounts()
        {
            var accounts = new List<AccountElement>();   
            foreach (var account in Application.Accounts)
            {
                var thisAccount = account;
                var t = new AccountElement(thisAccount);
                t.Tapped += () => AccountSelected(thisAccount);

                //Check to see if this account is the active account. Application.Account could be null 
                //so make it the target of the equals, not the source.
                if (thisAccount.Equals(Application.Account))
                    t.Accessory = UITableViewCellAccessory.Checkmark;
                accounts.Add(t);
            }
            return accounts;
        }


        protected override void AddAccountClicked()
        {
            NavigationController.PushViewController(new AccountTypeViewController(), true);
        }

        protected override void AccountDeleted(IAccount account)
        {
            //Remove the designated username
            var thisAccount = (Account)account;
            Application.Accounts.Remove(thisAccount);

            if (Application.Account != null && Application.Account.Equals(thisAccount))
            {
                NavigationItem.LeftBarButtonItem.Enabled = false;
                Application.Account = null;
            }
        }

        private async void AccountSelected(Account account)
        {
            var a = account as Account;
            //If the account doesn't remember the password we need to prompt
            if (a.DontRemember)
            {
                NavigationController.PushViewController(new LoginViewController(account), true);
            }
            //Change the user!
            else
            {
                try
                {
                    await Utils.Login.LoginAccount(account.Domain, account.Username, a.Password, this);
                }
                catch (Exception e)
                {
                    MonoTouch.Utilities.ShowAlert("Error", e.Message);
                    NavigationController.PushViewController(new LoginViewController(account), true);
                }
            }
        }
    }
}

