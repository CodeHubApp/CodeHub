using ReactiveUI;
using CodeHub.iOS.Cells;
using System;
using Foundation;
using CodeHub.Core.ViewModels.Activity;
using UIKit;
using CodeHub.iOS.Utilities;

namespace CodeHub.iOS.TableViewSources
{
    public class EventTableViewSource : ReactiveTableViewSource<EventItemViewModel>
    {
        private readonly TableViewCellHeightCache<NewsCellView, EventItemViewModel> _cache;

        public EventTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<EventItemViewModel> collection) 
            : base(tableView, collection,  NewsCellView.Key, 100f)
        {
            _cache = new TableViewCellHeightCache<NewsCellView, EventItemViewModel>(100f, NewsCellView.Create);
            tableView.SeparatorInset = NewsCellView.EdgeInsets;
            tableView.RegisterNibForCellReuse(NewsCellView.Nib, NewsCellView.Key);
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);
            var item = ItemAt(indexPath) as EventItemViewModel;
            if (item != null)
                item.GoToCommand.ExecuteIfCan();
        }

        public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
        {
            return _cache[indexPath];
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            var item = ItemAt(indexPath) as EventItemViewModel;
            return item != null ? _cache.GenerateHeight(tableView, item, indexPath) : base.GetHeightForRow(tableView, indexPath);
        }
    }
}

