using ReactiveUI;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.Cells;

namespace CodeHub.iOS.TableViewSources
{
    public class RepositoryTableViewSource : ReactiveTableViewSource<RepositoryItemViewModel>
    {
        private RepositoryCellView _usedForHeight;

        public RepositoryTableViewSource(MonoTouch.UIKit.UITableView tableView, IReactiveNotifyCollectionChanged<RepositoryItemViewModel> collection) 
            : base(tableView, collection,  RepositoryCellView.Key, 64.0f)
        {
            tableView.RegisterNibForCellReuse(RepositoryCellView.Nib, RepositoryCellView.Key);
        }

        public RepositoryTableViewSource(MonoTouch.UIKit.UITableView tableView) 
            : base(tableView, 64f)
        {
            tableView.RegisterNibForCellReuse(RepositoryCellView.Nib, RepositoryCellView.Key);
        }

        public override float GetHeightForRow(MonoTouch.UIKit.UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            if (_usedForHeight == null)
                _usedForHeight = RepositoryCellView.Create();

            var item = ItemAt(indexPath) as RepositoryItemViewModel;
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
            var item = ItemAt(indexPath) as RepositoryItemViewModel;
            if (item != null)
                item.GoToCommand.ExecuteIfCan();
        }
    }
}

