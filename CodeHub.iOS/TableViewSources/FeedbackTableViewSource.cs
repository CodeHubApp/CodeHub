using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.App;
using System;
using UIKit;
using CodeHub.iOS.Utilities;
using Foundation;

namespace CodeHub.iOS.TableViewSources
{
    public class FeedbackTableViewSource : ReactiveTableViewSource<FeedbackItemViewModel>
    {
        private readonly TableViewCellHeightCache<FeedbackCellView, FeedbackItemViewModel> _cache;

        public FeedbackTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<FeedbackItemViewModel> collection) 
            : base(tableView, collection, FeedbackCellView.Key, 69.0f)
        {
            _cache = new TableViewCellHeightCache<FeedbackCellView, FeedbackItemViewModel>(69.0f, () => FeedbackCellView.Create(true));
            tableView.RegisterNibForCellReuse(FeedbackCellView.Nib, FeedbackCellView.Key);
        }

        public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
        {
            return _cache[indexPath];
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            var item = ItemAt(indexPath) as FeedbackItemViewModel;
            return item != null ? _cache.GenerateHeight(tableView, item, indexPath) : base.GetHeightForRow(tableView, indexPath);
        }
    }
}

