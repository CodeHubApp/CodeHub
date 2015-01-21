using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.PullRequests;
using System;
using UIKit;

namespace CodeHub.iOS.TableViewSources
{
    public class PullRequestTableViewSource : ReactiveTableViewSource<PullRequestItemViewModel>
    {
        public PullRequestTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<PullRequestItemViewModel> collection) 
            : base(tableView, collection,  PullRequestCellView.Key, 60.0f)
        {
            tableView.RegisterNibForCellReuse(PullRequestCellView.Nib, PullRequestCellView.Key);
        }

        public override nfloat GetHeightForRow(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            return UITableView.AutomaticDimension;
        }
    }
}

