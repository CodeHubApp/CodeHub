using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.Changesets;
using System;
using UIKit;
using CodeHub.iOS.Utilities;
using Foundation;

namespace CodeHub.iOS.TableViewSources
{
    public class CommitTableViewSource : ReactiveTableViewSource<CommitItemViewModel>
    {
        private readonly TableViewCellHeightCache<CommitCellView, CommitItemViewModel> _cache;

        public CommitTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<CommitItemViewModel> collection) 
            : base(tableView, collection,  CommitCellView.Key, 64.0f)
        {
            _cache = new TableViewCellHeightCache<CommitCellView, CommitItemViewModel>(64f, new Lazy<CommitCellView>(CommitCellView.Create));
            tableView.RegisterNibForCellReuse(CommitCellView.Nib, CommitCellView.Key);
        }

        public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
        {
            return _cache[indexPath];
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            var item = ItemAt(indexPath) as CommitItemViewModel;
            return item != null ? _cache.GenerateHeight(tableView, item, indexPath) : base.GetHeightForRow(tableView, indexPath);
        }
    }
}

