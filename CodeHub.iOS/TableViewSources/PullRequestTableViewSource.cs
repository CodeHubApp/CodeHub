using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.PullRequests;
using System;

namespace CodeHub.iOS.TableViewSources
{
    public class PullRequestTableViewSource : ReactiveTableViewSource<PullRequestItemViewModel>
    {
        private PullRequestCellView _usedForHeight;

        public PullRequestTableViewSource(UIKit.UITableView tableView, IReactiveNotifyCollectionChanged<PullRequestItemViewModel> collection) 
            : base(tableView, collection,  PullRequestCellView.Key, 60.0f)
        {
            tableView.RegisterNibForCellReuse(PullRequestCellView.Nib, PullRequestCellView.Key);
        }

        public PullRequestTableViewSource(UIKit.UITableView tableView) 
            : base(tableView)
        {
            tableView.RegisterNibForCellReuse(PullRequestCellView.Nib, PullRequestCellView.Key);
        }

        public override nfloat GetHeightForRow(UIKit.UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            if (_usedForHeight == null)
                _usedForHeight = PullRequestCellView.Create();

            var item = ItemAt(indexPath) as PullRequestItemViewModel;
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
            var item = ItemAt(indexPath) as PullRequestItemViewModel;
            if (item != null)
                item.GoToCommand.ExecuteIfCan();
        }
    }
}

