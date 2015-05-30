using CodeHub.Core.ViewModels.Issues;
using System;
using UIKit;
using CodeHub.iOS.ViewComponents;
using CodeHub.iOS.TableViewSources;
using ReactiveUI;
using System.Reactive.Linq;

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

            this.WhenAnyValue(x => x.ViewModel.Assignees)
                .Select(x => new IssueAssigneeTableViewSource(TableView, x))
                .BindTo(TableView, x => x.Source);
        }
    }
}

