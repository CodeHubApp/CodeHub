using System;
using ReactiveUI;
using CodeHub.iOS.Cells;
using UIKit;
using CodeHub.Core.ViewModels.Issues;

namespace CodeHub.iOS.TableViewSources
{
    public class IssueLabelTableViewSource : ReactiveTableViewSource<IssueLabelItemViewModel>
    {
        public IssueLabelTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<IssueLabelItemViewModel> collection) 
            : base(tableView, collection,  IssueLabelCellView.Key, 44f)
        {
            tableView.RegisterClassForCellReuse(typeof(IssueLabelCellView), IssueLabelCellView.Key);
        }

        public override void RowSelected(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);
            var item = ItemAt(indexPath) as IssueLabelItemViewModel;
            if (item != null)
                item.SelectCommand.ExecuteIfCan();
        }
    }
}

