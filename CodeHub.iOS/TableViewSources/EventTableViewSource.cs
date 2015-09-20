using ReactiveUI;
using CodeHub.iOS.Cells;
using Foundation;
using CodeHub.Core.ViewModels.Activity;
using UIKit;

namespace CodeHub.iOS.TableViewSources
{
    public class EventTableViewSource : ReactiveTableViewSource<EventItemViewModel>
    {
        public EventTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<EventItemViewModel> collection) 
            : base(tableView, collection,  NewsCellView.Key, UITableView.AutomaticDimension, 100f)
        {
            tableView.RegisterNibForCellReuse(NewsCellView.Nib, NewsCellView.Key);
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);
            var item = ItemAt(indexPath) as EventItemViewModel;
            if (item != null)
                item.GoToCommand.ExecuteIfCan();
        }
    }
}

