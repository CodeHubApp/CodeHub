using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.Issues;

namespace CodeHub.iOS.TableViewSources
{
    public class IssueAssigneeTableViewSource : ReactiveTableViewSource<IssueAssigneeItemViewModel>
    {
        public IssueAssigneeTableViewSource(UIKit.UITableView tableView, IReactiveNotifyCollectionChanged<IssueAssigneeItemViewModel> collection) 
            : base(tableView, collection,  IssueAssigneeTableViewCell.Key, 44.0f)
        {
            tableView.RegisterClassForCellReuse(typeof(IssueAssigneeTableViewCell), IssueAssigneeTableViewCell.Key);
        }
    }
}

