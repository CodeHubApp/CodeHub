using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.Core.ViewModels.App;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.App
{
    public class AccountsView : BaseTableViewController<AccountsViewModel>
	{
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.RightBarButtonItem = ViewModel.GoToAddAccountCommand.ToBarButtonItem(UIBarButtonSystemItem.Add);
            NavigationItem.LeftBarButtonItem = new UIBarButtonItem { Image = Images.Cancel }.WithCommand(ViewModel.DismissCommand);
            TableView.Source = new AccountTableViewSource(TableView, ViewModel.Accounts);
        }
    }
}

