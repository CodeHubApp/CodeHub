using CodeHub.Core.ViewModels.Issues;
using System;
using UIKit;
using CodeHub.iOS.ViewComponents;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueAssignedToView : BaseTableViewController<IssueAssignedToViewModel>
    {
        public IssueAssignedToView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Person.ToImage(64f), "There are no assignees."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new UserTableViewSource(TableView, ViewModel.Users);
        }
    }
}

