using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.Changesets;
using UIKit;

namespace CodeHub.iOS.TableViewSources
{
    public class CommitTableViewSource : ReactiveTableViewSource<CommitItemViewModel>
    {
        public CommitTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<CommitItemViewModel> collection) 
            : base(tableView, collection,  CommitCellView.Key, UITableView.AutomaticDimension, 64.0f)
        {
            tableView.RegisterNibForCellReuse(CommitCellView.Nib, CommitCellView.Key);
        }
    }
}

