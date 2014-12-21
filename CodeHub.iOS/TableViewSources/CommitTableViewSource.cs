using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.Changesets;

namespace CodeHub.iOS.TableViewSources
{
    public class CommitTableViewSource : ReactiveTableViewSource<CommitItemViewModel>
    {
        private CommitCellView _usedForHeight;

        public CommitTableViewSource(MonoTouch.UIKit.UITableView tableView, IReactiveNotifyCollectionChanged<CommitItemViewModel> collection) 
            : base(tableView, collection,  CommitCellView.Key, 64.0f)
        {
            tableView.RegisterNibForCellReuse(CommitCellView.Nib, CommitCellView.Key);
        }

        public override float GetHeightForRow(MonoTouch.UIKit.UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            if (_usedForHeight == null)
                _usedForHeight = CommitCellView.Create();

            var item = ItemAt(indexPath) as CommitItemViewModel;
            if (item != null)
            {
                _usedForHeight.ViewModel = item;
                _usedForHeight.SetNeedsUpdateConstraints();
                _usedForHeight.UpdateConstraintsIfNeeded();
                _usedForHeight.Bounds = new System.Drawing.RectangleF(0, 0, tableView.Bounds.Width, tableView.Bounds.Height);
                _usedForHeight.SetNeedsLayout();
                _usedForHeight.LayoutIfNeeded();
                return _usedForHeight.ContentView.SystemLayoutSizeFittingSize(MonoTouch.UIKit.UIView.UILayoutFittingCompressedSize).Height + 1;
            }

            return base.GetHeightForRow(tableView, indexPath);
        }

        public override void RowSelected(MonoTouch.UIKit.UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);
            var item = ItemAt(indexPath) as CommitItemViewModel;
            if (item != null)
                item.GoToCommand.ExecuteIfCan();
        }
    }
}

