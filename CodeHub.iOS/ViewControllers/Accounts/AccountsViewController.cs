using UIKit;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using CodeHub.Core.ViewModels.Accounts;

namespace CodeHub.iOS.ViewControllers.Accounts
{
    public class AccountsViewController : BaseTableViewController<AccountsViewModel>, IModalViewController
	{
        public AccountsViewController()
        {
            OnActivation(d => {
                d(this.WhenAnyValue(x => x.ViewModel.GoToAddAccountCommand)
                    .ToBarButtonItem(UIBarButtonSystemItem.Add, x => NavigationItem.RightBarButtonItem = x));

                d(this.WhenAnyValue(x => x.ViewModel.DismissCommand)
                    .ToBarButtonItem(Images.Cancel, x => NavigationItem.LeftBarButtonItem = x));
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new AccountTableViewSource(TableView, ViewModel.Items);
        }
    }
}

