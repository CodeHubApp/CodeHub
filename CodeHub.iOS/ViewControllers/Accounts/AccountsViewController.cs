using CodeHub.Core.Services;
using UIKit;
using Foundation;
using System;
using CodeHub.Core.Data;
using CoreGraphics;
using CodeHub.iOS.DialogElements;
using System.Linq;
using ReactiveUI;
using CodeHub.Core.Messages;
using Splat;
using System.Threading.Tasks;

namespace CodeHub.iOS.ViewControllers.Accounts
{
    public class AccountsViewController : DialogViewController
    {
        private readonly IAccountsService _accountsService = Locator.Current.GetService<IAccountsService>();
        private readonly IApplicationService _applicationService = Locator.Current.GetService<IApplicationService>();

        public AccountsViewController() : base(UITableViewStyle.Plain)
        {
            Title = "Accounts";

            var addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add);
            NavigationItem.RightBarButtonItem = addButton;
            OnActivation(d => d(addButton.GetClickedObservable().Subscribe(_ => AddAccount())));
        }

        private void AddAccount()
        {
            NavigationController.PushViewController(new NewAccountViewController(), true);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.RowHeight = 74;
        }

        private async Task SelectAccount(Account account)
        {
            await _accountsService.SetActiveAccount(account);
            MessageBus.Current.SendMessage(new LogoutMessage());
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            var weakVm = new WeakReference<AccountsViewController>(this);
            var accountSection = new Section();

            var activeAccount = _accountsService.GetActiveAccount().Result;
            accountSection.AddAll(_accountsService.GetAccounts().Result.Select(account =>
            {
                var isEqual = account.Id == activeAccount?.Id;
                var t = new AccountElement(account, isEqual);
                t.Tapped += () => weakVm.Get()?.SelectAccount(account);
                return t;
            }));
            Root.Reset(accountSection);

            SetCancelButton();
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
            _accountsService.Remove(accountElement.Account);
            var activeAccount = _accountsService.GetActiveAccount().Result;

            if (activeAccount != null && activeAccount.Equals(accountElement.Account))
            {
                _applicationService.DeactivateUser();   
            }

            SetCancelButton();
        }

        private void SetCancelButton()
        {
            if (NavigationItem.LeftBarButtonItem != null)
            {
                NavigationItem.LeftBarButtonItem.Enabled = Root.Sum(x => x.Elements.Count) > 0 && _applicationService.Account != null;
            }
        }

        private class EditSource : DialogViewController.Source
        {
            public EditSource(AccountsViewController dvc) : base (dvc)
            {
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return true;
            }

            public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return UITableViewCellEditingStyle.Delete;
            }

            public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
            {
                if (indexPath == null)
                    return;

                switch (editingStyle)
                {
                    case UITableViewCellEditingStyle.Delete:
                        var section = Container.Root[indexPath.Section];
                        var element = section[indexPath.Row];
                        section.Remove(element);
                        (Container as AccountsViewController)?.Delete(element);
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

            public Account Account { get; private set; }

            public AccountElement(Account account, bool currentAccount)
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
                cell.ImageView.TintColor = Theme.CurrentTheme.PrimaryColor;
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

