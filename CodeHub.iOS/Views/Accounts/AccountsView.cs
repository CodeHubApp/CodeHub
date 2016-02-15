using System.Collections.Generic;
using MvvmCross.Platform;
using CodeHub.Core.Services;
using CodeHub.iOS.ViewControllers;
using UIKit;
using Foundation;
using System;
using CodeHub.Core.Data;
using CoreGraphics;
using CodeHub.Core.ViewModels.Accounts;
using CodeHub.iOS.DialogElements;
using System.Linq;
using System.Reactive.Disposables;

namespace CodeHub.iOS.Views.Accounts
{
	public class AccountsView : ViewModelDrivenDialogViewController
    {
        public new AccountsViewModel ViewModel
        {
            get { return (AccountsViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public AccountsView() : base(true, UITableViewStyle.Plain)
        {
            Title = "Accounts";

            var addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add);
            NavigationItem.RightBarButtonItem = addButton;
            NavigationItem.LeftBarButtonItem = null;

            OnActivation(d => {
                d(addButton.GetClickedObservable().BindCommand(ViewModel.AddAccountCommand));

                Console.WriteLine("Activated Accounts");
                d(Disposable.Create(() => Console.WriteLine("Deactivated Accounts")));
            });
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
            TableView.RowHeight = 74;
		}

        /// <summary>
        /// Called when the accounts need to be populated
        /// </summary>
        /// <returns>The accounts.</returns>
        protected IEnumerable<AccountElement> PopulateAccounts()
        {
            var accountsService = Mvx.Resolve<IAccountsService>();
            var weakVm = new WeakReference<AccountsViewModel>(ViewModel);

            return accountsService.Select(account =>
            {
                var t = new AccountElement(account, account.Equals(accountsService.ActiveAccount));
                t.Tapped += () => weakVm.Get()?.SelectAccountCommand.Execute(account);
                return t;
            });
        }

        /// <summary>
        /// Called when an account is deleted
        /// </summary>
        /// <param name="account">Account.</param>
        protected void AccountDeleted(GitHubAccount account)
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

            var accountSection = new Section();
            accountSection.AddAll(PopulateAccounts());
            Root.Reset(accountSection);
        }

		public override DialogViewController.Source CreateSizingSource()
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

		private class EditSource : DialogViewController.Source
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
                cell.ImageView.SetAvatar(new CodeHub.Core.Utilities.GitHubAvatar(Account.AvatarUrl));
                return cell;
            }

            public override void Selected (UITableView tableView, NSIndexPath indexPath)
            {
                base.Selected(tableView, indexPath);
                Tapped?.Invoke();
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

