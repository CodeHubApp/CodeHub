using System;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Accounts;

namespace CodeHub.iOS.ViewControllers.Accounts
{
    public class AccountsViewController : BaseTableViewController<AccountsViewModel>, IModalViewController
	{
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.WhenAnyValue(x => x.ViewModel.GoToAddAccountCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Add))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            this.WhenAnyValue(x => x.ViewModel.DismissCommand)
                .Select(x => x.ToBarButtonItem(Images.Cancel))
                .Subscribe(x => NavigationItem.LeftBarButtonItem = x);

            this.WhenAnyValue(x => x.ViewModel.Items)
                .Select(x => new AccountTableViewSource(TableView, x))
                .BindTo(TableView, x => x.Source);
        }
    }
}

