using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.PullRequests;

namespace CodeHub.iOS.TableViewSources
{
    public class PullRequestTableViewSource : ReactiveTableViewSource<PullRequestItemViewModel>
    {
        private PullRequestCellView _usedForHeight;

        public PullRequestTableViewSource(MonoTouch.UIKit.UITableView tableView, IReactiveNotifyCollectionChanged<PullRequestItemViewModel> collection) 
            : base(tableView, collection,  PullRequestCellView.Key, 60.0f)
        {
            tableView.RegisterNibForCellReuse(PullRequestCellView.Nib, PullRequestCellView.Key);
        }

        public PullRequestTableViewSource(MonoTouch.UIKit.UITableView tableView) 
            : base(tableView)
        {
            tableView.RegisterNibForCellReuse(PullRequestCellView.Nib, PullRequestCellView.Key);
        }

        public override float GetHeightForRow(MonoTouch.UIKit.UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            if (_usedForHeight == null)
                _usedForHeight = PullRequestCellView.Create();

            var item = ItemAt(indexPath) as PullRequestItemViewModel;
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
            var item = ItemAt(indexPath) as PullRequestItemViewModel;
            if (item != null)
                item.GoToCommand.ExecuteIfCan();
        }
    }
}

