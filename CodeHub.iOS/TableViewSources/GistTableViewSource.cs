using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.Gists;

namespace CodeHub.iOS.TableViewSources
{
    public class GistTableViewSource : ReactiveTableViewSource<GistItemViewModel>
    {
        private GistCellView _usedForHeight;

        public GistTableViewSource(MonoTouch.UIKit.UITableView tableView, IReactiveNotifyCollectionChanged<GistItemViewModel> collection) 
            : base(tableView, collection,  GistCellView.Key, 60.0f)
        {
            tableView.RegisterNibForCellReuse(GistCellView.Nib, GistCellView.Key);
        }

        public GistTableViewSource(MonoTouch.UIKit.UITableView tableView) 
            : base(tableView)
        {
            tableView.RegisterNibForCellReuse(GistCellView.Nib, GistCellView.Key);
        }

        public override float GetHeightForRow(MonoTouch.UIKit.UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            if (_usedForHeight == null)
                _usedForHeight = GistCellView.Create();

            var item = ItemAt(indexPath) as GistItemViewModel;
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
            var item = ItemAt(indexPath) as GistItemViewModel;
            if (item != null)
                item.GoToCommand.ExecuteIfCan();
        }
    }
}

