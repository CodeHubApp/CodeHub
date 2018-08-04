using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Data;
using CodeHub.Core.Messages;
using CodeHub.Core.Services;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.TableViewSources;
using CoreGraphics;
using Foundation;
using ReactiveUI;
using Splat;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Accounts
{
    public class AccountsViewController : TableViewController
    {
        private readonly IAccountsService _accountsService = Locator.Current.GetService<IAccountsService>();
        private readonly IApplicationService _applicationService = Locator.Current.GetService<IApplicationService>();
        private readonly ReactiveCommand<Unit, Unit> _loadCommand;
        private readonly ReactiveCommand<Account, Unit> _selectCommand;
        private readonly ReactiveCommand<Account, Unit> _deleteCommand;
        private readonly EditSource _source;

        public AccountsViewController()
        {
            Title = "Accounts";
            _source = new EditSource(this);

            _selectCommand = ReactiveCommand.CreateFromTask<Account>(async (account) =>
            {
                await _accountsService.SetActiveAccount(account);
                MessageBus.Current.SendMessage(new LogoutMessage());
            });

            _loadCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var activeAccount = await _accountsService.GetActiveAccount();
                var accounts = await _accountsService.GetAccounts();

                var elements = accounts.Select(account =>
                {
                    var isEqual = account.Id == activeAccount?.Id;
                    var element = new AccountElement(account, isEqual);

                    element
                        .Clicked
                        .Select(_ => account)
                        .InvokeReactiveCommand(_selectCommand);

                    return element;
                });

                _source.Root.Reset(new Section { elements });
            });

            _deleteCommand = ReactiveCommand.CreateFromTask<Account>(async (account) =>
            {
                await _accountsService.Remove(account);
                var activeAccount = await _accountsService.GetActiveAccount();

                if (activeAccount != null && activeAccount.Equals(account))
                    _applicationService.DeactivateUser();
            });

            var addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add);
            NavigationItem.RightBarButtonItem = addButton;

            OnActivation(d => 
            {
                d(addButton
                  .GetClickedObservable()
                  .Select(_ => new NewAccountViewController())
                  .Subscribe(this.PushViewController));

                d(_deleteCommand
                  .Merge(_loadCommand)
                  .Subscribe(_ => SetCancelButton()));
            });

            Appearing
                .Select(_ => Unit.Default)
                .InvokeReactiveCommand(_loadCommand);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.Source = _source;
            TableView.RowHeight = 74;
        }

        private void SetCancelButton()
        {
            if (NavigationItem.LeftBarButtonItem != null)
            {
                NavigationItem.LeftBarButtonItem.Enabled = 
                    _source.Root.Sum(x => x.Elements.Count) > 0 && 
                    _applicationService.Account != null;
            }
        }

        private class EditSource : DialogTableViewSource
        {
            private readonly WeakReference<AccountsViewController> _viewCtrl;

            public EditSource(AccountsViewController viewCtrl)
                : base (viewCtrl.TableView)
            {
                _viewCtrl = new WeakReference<AccountsViewController>(viewCtrl);
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
                        var section = Root[indexPath.Section];
                        var element = section[indexPath.Row];
                        section.Remove(element);

                        var accountElement = element as AccountElement;
                        if (accountElement != null)
                            _viewCtrl.Get()._deleteCommand.ExecuteNow(accountElement.Account);

                        break;
                }
            }
        }

        /// <summary>
        /// An element that represents an account object
        /// </summary>
        protected class AccountElement : Element
        {
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

