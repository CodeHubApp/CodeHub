using CodeHub.Core.ViewModels.Organizations;
using CodeHub.iOS.TableViewSources;
using UIKit;
using System;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Organizations
{
    public class OrganizationsViewController : BaseTableViewController<OrganizationsViewModel>
    {
        public OrganizationsViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Organization.ToEmptyListImage(), "There are no organizations."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new UserTableViewSource(TableView, ViewModel.Items);
        }
	}
}

