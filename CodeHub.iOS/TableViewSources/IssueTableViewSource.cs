using ReactiveUI;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.iOS.Cells;

namespace CodeHub.iOS.TableViewSources
{
    public class IssueTableViewSource : ReactiveTableViewSource<IssueItemViewModel>
    {
        public IssueTableViewSource(MonoTouch.UIKit.UITableView tableView, IReactiveNotifyCollectionChanged<IssueItemViewModel> collection) 
            : base(tableView, collection, IssueCellView.Key, 69.0f)
        {
            tableView.RegisterNibForCellReuse(IssueCellView.Nib, IssueCellView.Key);
        }

        public IssueTableViewSource(MonoTouch.UIKit.UITableView tableView) 
            : base(tableView, 69.0f)
        {
            tableView.RegisterNibForCellReuse(IssueCellView.Nib, IssueCellView.Key);
        }

        public override void RowSelected(MonoTouch.UIKit.UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);
            var item = ItemAt(indexPath) as IssueItemViewModel;
            if (item != null)
                item.GoToCommand.ExecuteIfCan();
        }
    }
}

