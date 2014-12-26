using System;
using MonoTouch.UIKit;
using System.Reactive.Subjects;
using System.Reactive;

namespace CodeHub.iOS.TableViewSources
{
    public abstract class ReactiveTableViewSource<TViewModel> : ReactiveUI.ReactiveTableViewSource<TViewModel>, IInformsEnd
    {
        private readonly ISubject<Unit> _requestMoreSubject = new Subject<Unit>();
        private readonly float _estimatedHeight;

        public IObservable<Unit> RequestMore
        {
            get { return _requestMoreSubject; }
        }

        protected ReactiveTableViewSource(UITableView tableView, float? sizeHint = null)
            : base(tableView)
        {
            _estimatedHeight = sizeHint ?? UITableView.AutomaticDimension;
        }

        protected ReactiveTableViewSource(UITableView tableView, ReactiveUI.IReactiveNotifyCollectionChanged<TViewModel> collection, 
            MonoTouch.Foundation.NSString cellKey, float sizeHint, Action<UITableViewCell> initializeCellAction = null) 
            : base(tableView, collection, cellKey, sizeHint, initializeCellAction)
        {
            _estimatedHeight = sizeHint;
        }

        public override void WillDisplay(UITableView tableView, UITableViewCell cell, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            if (indexPath.Section == (NumberOfSections(tableView) - 1) &&
                indexPath.Row == (RowsInSection(tableView, indexPath.Section) - 1))
            {
                // We need to skip an event loop to stay out of trouble
                BeginInvokeOnMainThread(() => _requestMoreSubject.OnNext(Unit.Default));
            }
        }

        public override float EstimatedHeight(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            return _estimatedHeight;
        }

        public override void RowSelected(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);
            tableView.DeselectRow(indexPath, true);
        }
    }

    public interface IInformsEnd
    {
        IObservable<Unit> RequestMore { get; }
    }
}

