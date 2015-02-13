using CodeHub.Core.ViewModels.Issues;
using System;
using UIKit;
using CodeHub.iOS.ViewComponents;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueAssigneeView : BaseTableViewController<IssueAssigneeViewModel>
    {
        public IssueAssigneeView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Person.ToEmptyListImage(), "There are no assignees."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new IssueAssigneeTableViewSource(TableView, ViewModel.Assignees);
        }
    }
}

