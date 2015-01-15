using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.Gists;
using System;

namespace CodeHub.iOS.TableViewSources
{
    public class GistTableViewSource : ReactiveTableViewSource<GistItemViewModel>
    {
        private GistCellView _usedForHeight;

        public GistTableViewSource(UIKit.UITableView tableView, IReactiveNotifyCollectionChanged<GistItemViewModel> collection) 
            : base(tableView, collection,  GistCellView.Key, 60.0f)
        {
            tableView.RegisterNibForCellReuse(GistCellView.Nib, GistCellView.Key);
        }

        public override nfloat GetHeightForRow(UIKit.UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            if (_usedForHeight == null)
                _usedForHeight = GistCellView.Create();

            var item = ItemAt(indexPath) as GistItemViewModel;
            if (item != null)
            {
                _usedForHeight.ViewModel = item;
                _usedForHeight.SetNeedsUpdateConstraints();
                _usedForHeight.UpdateConstraintsIfNeeded();
                _usedForHeight.Bounds = new CoreGraphics.CGRect(0, 0, tableView.Bounds.Width, tableView.Bounds.Height);
                _usedForHeight.SetNeedsLayout();
                _usedForHeight.LayoutIfNeeded();
                return _usedForHeight.ContentView.SystemLayoutSizeFittingSize(UIKit.UIView.UILayoutFittingCompressedSize).Height + 1;
            }

            return base.GetHeightForRow(tableView, indexPath);
        }

        public override void RowSelected(UIKit.UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);
            var item = ItemAt(indexPath) as GistItemViewModel;
            if (item != null)
                item.GoToCommand.ExecuteIfCan();
        }
    }
}

