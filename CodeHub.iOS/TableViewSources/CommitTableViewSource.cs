using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.Changesets;
using System;
using UIKit;

namespace CodeHub.iOS.TableViewSources
{
    public class CommitTableViewSource : ReactiveTableViewSource<CommitItemViewModel>
    {
        public CommitTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<CommitItemViewModel> collection) 
            : base(tableView, collection,  CommitCellView.Key, 64.0f)
        {
            tableView.RegisterNibForCellReuse(CommitCellView.Nib, CommitCellView.Key);
        }

        public override nfloat GetHeightForRow(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            return UITableView.AutomaticDimension;
        }
    }
}

