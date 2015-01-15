using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.Releases;

namespace CodeHub.iOS.TableViewSources
{
    public class ReleasesTableViewSource : ReactiveTableViewSource<ReleaseItemViewModel>
    {
        public ReleasesTableViewSource(UIKit.UITableView tableView, IReactiveNotifyCollectionChanged<ReleaseItemViewModel> collection) 
            : base(tableView, collection,  ReleaseTableViewCell.Key, 54.0f)
        {
            tableView.RegisterClassForCellReuse(typeof(ReleaseTableViewCell), ReleaseTableViewCell.Key);
        }

        public override void RowSelected(UIKit.UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);
            var item = ItemAt(indexPath) as ReleaseItemViewModel;
            if (item != null)
                item.GoToCommand.ExecuteIfCan();
        }
    }
}

