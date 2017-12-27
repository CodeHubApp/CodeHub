using ReactiveUI;
using CodeHub.iOS.TableViewCells;
using CodeHub.Core.ViewModels.Changesets;
using UIKit;

namespace CodeHub.iOS.TableViewSources
{
    public class CommitsTableViewSource : ReactiveTableViewSource<CommitItemViewModel>
    {
        public CommitsTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<CommitItemViewModel> collection)
            : base(tableView, collection, CommitCellView.Key, UITableView.AutomaticDimension, 65f)
        {
            tableView.RegisterNibForCellReuse(CommitCellView.Nib, CommitCellView.Key);
        }
    }
}

