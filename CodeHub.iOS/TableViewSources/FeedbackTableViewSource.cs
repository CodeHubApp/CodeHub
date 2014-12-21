using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.App;
using System.Drawing;
using System;
using System.Reactive.Subjects;
using MonoTouch.UIKit;

namespace CodeHub.iOS.TableViewSources
{
    public class FeedbackTableViewSource : ReactiveTableViewSource<FeedbackItemViewModel>
    {
        private FeedbackCellView _usedForHeight;
        private UITableView _tableView;
        private readonly Subject<PointF> _scrolledSubject = new Subject<PointF>();

        public IObservable<PointF> ScrolledObservable { get { return _scrolledSubject; } }

        public FeedbackTableViewSource(MonoTouch.UIKit.UITableView tableView, IReactiveNotifyCollectionChanged<FeedbackItemViewModel> collection) 
            : base(tableView, collection, FeedbackCellView.Key, 69.0f)
        {
            _tableView = tableView;
            tableView.RegisterNibForCellReuse(FeedbackCellView.Nib, FeedbackCellView.Key);
        }

        public FeedbackTableViewSource(MonoTouch.UIKit.UITableView tableView) 
            : base(tableView, 69.0f)
        {
            tableView.RegisterNibForCellReuse(FeedbackCellView.Nib, FeedbackCellView.Key);
        }

        public override void Scrolled(MonoTouch.UIKit.UIScrollView scrollView)
        {
            _scrolledSubject.OnNext(_tableView.ContentOffset);
        }

        public override float GetHeightForRow(MonoTouch.UIKit.UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            if (_usedForHeight == null)
                _usedForHeight = FeedbackCellView.Create();

            var item = ItemAt(indexPath) as FeedbackItemViewModel;
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
            var item = ItemAt(indexPath) as FeedbackItemViewModel;
            if (item != null)
                item.GoToCommand.ExecuteIfCan();
        }
    }
}

