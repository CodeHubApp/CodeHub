using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.PullRequests;
using System;
using UIKit;
using CodeHub.iOS.Utilities;
using Foundation;

namespace CodeHub.iOS.TableViewSources
{
    public class PullRequestTableViewSource : ReactiveTableViewSource<PullRequestItemViewModel>
    {
        private readonly TableViewCellHeightCache<PullRequestCellView, PullRequestItemViewModel> _cache;

        public PullRequestTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<PullRequestItemViewModel> collection) 
            : base(tableView, collection,  PullRequestCellView.Key, 60.0f)
        {
            _cache = new TableViewCellHeightCache<PullRequestCellView, PullRequestItemViewModel>(60f, new Lazy<PullRequestCellView>(PullRequestCellView.Create));
            tableView.RegisterNibForCellReuse(PullRequestCellView.Nib, PullRequestCellView.Key);
        }

        public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
        {
            return _cache[indexPath];
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            var item = ItemAt(indexPath) as PullRequestItemViewModel;
            return item != null ? _cache.GenerateHeight(tableView, item, indexPath) : base.GetHeightForRow(tableView, indexPath);
        }
    }
}

