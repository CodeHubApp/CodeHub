using CodeHub.Core.ViewModels.Organizations;
using CodeHub.iOS.TableViewSources;
using UIKit;
using System;
using CodeHub.iOS.Views;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.iOS.ViewControllers.Organizations
{
    public class OrganizationsViewController : BaseTableViewController<OrganizationsViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Organization.ToEmptyListImage(), "There are no organizations."));

            this.WhenAnyValue(x => x.ViewModel.Organizations)
                .Select(x => new UserTableViewSource(TableView, x))
                .BindTo(TableView, x => x.Source);
        }
	}
}

