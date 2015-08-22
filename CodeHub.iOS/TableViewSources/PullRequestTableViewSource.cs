using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.PullRequests;
using UIKit;

namespace CodeHub.iOS.TableViewSources
{
    public class PullRequestTableViewSource : ReactiveTableViewSource<PullRequestItemViewModel>
    {
        public PullRequestTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<PullRequestItemViewModel> collection) 
            : base(tableView, collection,  PullRequestCellView.Key, UITableView.AutomaticDimension, 60.0f)
        {
            tableView.RegisterNibForCellReuse(PullRequestCellView.Nib, PullRequestCellView.Key);
        }
    }
}

