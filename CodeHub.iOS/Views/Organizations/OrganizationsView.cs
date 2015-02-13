using CodeHub.Core.ViewModels.Organizations;
using CodeHub.iOS.TableViewSources;
using UIKit;
using CodeHub.iOS.ViewComponents;
using System;

namespace CodeHub.iOS.Views.Organizations
{
    public class OrganizationsView : BaseTableViewController<OrganizationsViewModel>
    {
        public OrganizationsView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Organization.ToEmptyListImage(), "There are no organizations."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new UserTableViewSource(TableView, ViewModel.Organizations);
        }
	}
}

