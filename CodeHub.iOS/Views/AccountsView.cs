using System.Collections.Generic;
using Cirrious.CrossCore;
using CodeFramework.Core.Data;
using CodeFramework.Core.Services;
using CodeFramework.Core.ViewModels;
using CodeFramework.iOS.Elements;
using CodeFramework.iOS.ViewControllers;
using CodeFramework.ViewControllers;
using MonoTouch.Dialog;
using UIKit;
using CodeFramework.iOS.Utils;

namespace CodeFramework.iOS.Views
{
	public class AccountsView : ViewModelDrivenDialogViewController
    {
		private IHud _hud;

        public new BaseAccountsViewModel ViewModel
        {
            get { return (BaseAccountsViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public AccountsView() : base(true)
        {
        }

		public override void ViewDidLoad()
		{
		    Title = "Accounts";
			base.ViewDidLoad();

            NavigationItem.LeftBarButtonItem = null;
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Add, (s, e) => ViewModel.AddAccountCommand.Execute(null));

			_hud = new Hud(View);
			ViewModel.Bind(x => x.IsLoggingIn, x =>
			{
				if (x)
				{
					_hud.Show("Logging in...");
				}
				else
				{
					_hud.Hide();
				}
			});
		}

        /// <summary>
        /// Called when the accounts need to be populated
        /// </summary>
        /// <returns>The accounts.</returns>
        protected List<AccountElement> PopulateAccounts()
        {
            var accounts = new List<AccountElement>();
            var accountsService = Mvx.Resolve<IAccountsService>();

            foreach (var account in accountsService)
            {
                var thisAccount = account;
                var t = new AccountElement(thisAccount);
                t.Tapped += () => ViewModel.SelectAccountCommand.Execute(thisAccount);

                //Check to see if this account is the active account. Application.Account could be null 
                //so make it the target of the equals, not the source.
                if (thisAccount.Equals(accountsService.ActiveAccount))
                    t.Accessory = UITableViewCellAccessory.Checkmark;
                accounts.Add(t);
            }
            return accounts;
        }

        /// <summary>
        /// Called when an account is deleted
        /// </summary>
        /// <param name="account">Account.</param>
        protected void AccountDeleted(IAccount account)
        {
            //Remove the designated username
            var thisAccount = account;
            var accountsService = Mvx.Resolve<IAccountsService>();

            accountsService.Remove(thisAccount);

            if (accountsService.ActiveAccount != null && accountsService.ActiveAccount.Equals(thisAccount))
            {
                accountsService.SetActiveAccount(null);
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            var root = new RootElement(Title);
            var accountSection = new Section();
            accountSection.AddAll(PopulateAccounts());
            root.Add(accountSection);
            Root = root;
        }


		public override DialogViewController.Source CreateSizingSource(bool unevenRows)
        {
            return new EditSource(this);
        }

        private void Delete(Element element)
        {
            var accountElement = element as AccountElement;
            if (accountElement == null)
                return;

            //Remove the designated username
            AccountDeleted(accountElement.Account);
        }

		private class EditSource : BaseDialogViewController.Source
        {
            private readonly AccountsView _parent;
            public EditSource(AccountsView dvc) 
                : base (dvc)
            {
                _parent = dvc;
            }

            public override bool CanEditRow(UITableView tableView, Foundation.NSIndexPath indexPath)
            {
                return (indexPath.Section == 0);
            }

            public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, Foundation.NSIndexPath indexPath)
            {
                if (indexPath.Section == 0)
                    return UITableViewCellEditingStyle.Delete;
                return UITableViewCellEditingStyle.None;
            }

            public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, Foundation.NSIndexPath indexPath)
            {
                if (indexPath == null)
                    return;

                switch (editingStyle)
                {
                    case UITableViewCellEditingStyle.Delete:
                        var section = _parent.Root[indexPath.Section];
                        var element = section[indexPath.Row];
                        _parent.Delete(element);
                        section.Remove(element);
                        break;
                }
            }
        }

        /// <summary>
        /// An element that represents an account object
        /// </summary>
        protected class AccountElement : UserElement
        {
            public IAccount Account { get; private set; }

            public AccountElement(IAccount account)
                : base(account.Username, string.Empty, string.Empty, account.AvatarUrl)
            {
                Account = account;
            }
        }
    }
}

