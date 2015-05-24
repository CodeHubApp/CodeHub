using System;
using UIKit;
using ReactiveUI;
using CodeHub.Core.ViewModels.App;
using CodeHub.iOS.TableViewSources;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.App
{
    public class AccountsView : BaseTableViewController<AccountsViewModel>, IModalView
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

            this.WhenAnyValue(x => x.ViewModel.Accounts)
                .BindTableSource(TableView, (tv, x) => new AccountTableViewSource(tv, x));
        }
    }
}

