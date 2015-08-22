using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.App;
using UIKit;

namespace CodeHub.iOS.TableViewSources
{
    public class FeedbackTableViewSource : ReactiveTableViewSource<FeedbackItemViewModel>
    {
        public FeedbackTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<FeedbackItemViewModel> collection) 
            : base(tableView, collection, FeedbackCellView.Key, UITableView.AutomaticDimension, 69.0f)
        {
            tableView.RegisterNibForCellReuse(FeedbackCellView.Nib, FeedbackCellView.Key);
        }
    }
}

