using System.Collections.Generic;
using MvvmCross.Platform;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.ViewControllers;
using CodeFramework.ViewControllers;
using MonoTouch.Dialog;
using UIKit;
using CodeFramework.iOS.Utils;
using Foundation;
using System;
using CodeHub.Core.Data;
using CoreGraphics;
using System.Linq;
using SDWebImage;

namespace CodeHub.iOS.Views.Accounts
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
            Style = UITableViewStyle.Plain;
            Title = "Accounts";
            NavigationItem.LeftBarButtonItem = null;
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            TableView.RowHeight = 74;

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

            foreach (var account in accountsService.OfType<GitHubAccount>())
            {
                var thisAccount = account;
                var t = new AccountElement(thisAccount, thisAccount.Equals(accountsService.ActiveAccount));
                t.Tapped += () => ViewModel.SelectAccountCommand.Execute(thisAccount);
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

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Add, (s, e) => ViewModel.AddAccountCommand.Execute(null));

            var root = new RootElement(Title);
            var accountSection = new Section();
            accountSection.AddAll(PopulateAccounts());
            root.Add(accountSection);
            Root = root;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            NavigationItem.RightBarButtonItem = null;
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
        protected class AccountElement : Element
        {
            public event Action Tapped;

            private readonly bool _currentAccount;

            public GitHubAccount Account { get; private set; }

            public AccountElement(GitHubAccount account, bool currentAccount)
            {
                Account = account;
                _currentAccount = currentAccount;
            }

            public override UITableViewCell GetCell(UITableView tv)
            {
                var cell = (tv.DequeueReusableCell(AccountCellView.Key) as AccountCellView) ?? new AccountCellView();
                cell.Accessory = _currentAccount ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
                cell.TextLabel.Text = Account.Username;
                cell.DetailTextLabel.Text = Account.IsEnterprise ? Account.WebDomain : "GitHub.com";
                cell.ImageView.SetImage(new NSUrl(Account.AvatarUrl), Images.Avatar);
                return cell;
            }

            public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
            {
                Tapped?.Invoke();
                tableView.DeselectRow (indexPath, true);
            }
        }

        public class AccountCellView : UITableViewCell
        {
            public static NSString Key = new NSString("ProfileCell");


            public AccountCellView()
                : base(UITableViewCellStyle.Subtitle, Key)
            {
                ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
                ImageView.Layer.MinificationFilter = CoreAnimation.CALayer.FilterTrilinear;
                ImageView.Layer.MasksToBounds = true;

                TextLabel.TextColor = UIColor.FromWhiteAlpha(0.0f, 1f);
                TextLabel.Font = UIFont.FromName("HelveticaNeue", 17f);

                DetailTextLabel.TextColor = UIColor.FromWhiteAlpha(0.1f, 1f);
                DetailTextLabel.Font = UIFont.FromName("HelveticaNeue-Thin", 14f);
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                var imageSize = this.Bounds.Height - 30f;
                ImageView.Layer.CornerRadius = imageSize / 2;
                ImageView.Frame = new CGRect(15, 15, imageSize, imageSize);

                var titlePoint = new CGPoint(ImageView.Frame.Right + 15f, 19f);
                TextLabel.Frame = new CGRect(titlePoint.X, titlePoint.Y, this.ContentView.Bounds.Width - titlePoint.X - 10f, TextLabel.Font.LineHeight);
                DetailTextLabel.Frame = new CGRect(titlePoint.X, TextLabel.Frame.Bottom, this.ContentView.Bounds.Width - titlePoint.X - 10f, DetailTextLabel.Font.LineHeight + 1);

                SeparatorInset = new UIEdgeInsets(0, TextLabel.Frame.Left, 0, 0);
            }
        }
    }
}

