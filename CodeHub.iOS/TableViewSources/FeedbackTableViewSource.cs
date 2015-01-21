using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.App;
using System;
using UIKit;

namespace CodeHub.iOS.TableViewSources
{
    public class FeedbackTableViewSource : ReactiveTableViewSource<FeedbackItemViewModel>
    {
        public FeedbackTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<FeedbackItemViewModel> collection) 
            : base(tableView, collection, FeedbackCellView.Key, 69.0f)
        {
            tableView.RegisterNibForCellReuse(FeedbackCellView.Nib, FeedbackCellView.Key);
        }

        public override nfloat GetHeightForRow(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            return UITableView.AutomaticDimension;
        }
    }
}

