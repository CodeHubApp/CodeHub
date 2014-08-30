using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.Core.ViewModels.App;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.App
{
    public class AccountsView : ReactiveTableViewController<AccountsViewModel>
	{
        public override void ViewDidLoad()
        {
            Title = "Accounts";

            base.ViewDidLoad();

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Add).WithCommand(ViewModel.GoToAddAccountCommand);
            NavigationItem.LeftBarButtonItem = new UIBarButtonItem { Image = Images.Cancel }.WithCommand(ViewModel.DismissCommand);
            TableView.Source = new AccountTableViewSource(TableView, ViewModel.Accounts);
        }
    }
}

