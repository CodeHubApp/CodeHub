using CodeHub.Core.ViewModels.Organizations;
using CodeHub.iOS.TableViewSources;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Organizations
{
    public class OrganizationsView : ReactiveTableViewController<OrganizationsViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new UserTableViewSource(TableView, ViewModel.Organizations);
        }
	}
}

