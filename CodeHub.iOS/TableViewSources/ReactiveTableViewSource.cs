using System;
using UIKit;
using System.Reactive.Subjects;
using System.Reactive;
using CodeHub.Core.ViewModels;
using ReactiveUI;
using System.Linq;
using System.Reactive.Linq;
using CoreGraphics;

namespace CodeHub.iOS.TableViewSources
{
    public abstract class ReactiveTableViewSource<TViewModel> : ReactiveUI.ReactiveTableViewSource<TViewModel>, IInformsEnd, IInformsEmpty
    {
        private readonly ISubject<Unit> _requestMoreSubject = new Subject<Unit>();
        private readonly ISubject<bool> _isEmptySubject = new BehaviorSubject<bool>(true);
        private readonly ISubject<CGPoint> _scrollSubject = new Subject<CGPoint>();

        public IObservable<CGPoint> DidScroll
        {
            get { return _scrollSubject.AsObservable(); }
        }

        public IObservable<Unit> RequestMore
        {
            get { return _requestMoreSubject; }
        }

        public IObservable<bool> IsEmpty
        {
            get { return _isEmptySubject; }
        }

        public override void Scrolled(UIScrollView scrollView)
        {
            _scrollSubject.OnNext(scrollView.ContentOffset);
        }

        protected ReactiveTableViewSource(UITableView tableView, nfloat height, nfloat? heightHint = null)
            : base(tableView)
        {
            tableView.RowHeight = height;
            tableView.EstimatedRowHeight = heightHint ?? tableView.EstimatedRowHeight;
            this.WhenAnyValue(x => x.Data).IsNotNull().Select(x => x.Count == 0).Subscribe(_isEmptySubject.OnNext);
        }

        protected ReactiveTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<TViewModel> collection, 
            Foundation.NSString cellKey, nfloat height, nfloat? heightHint = null, Action<UITableViewCell> initializeCellAction = null) 
            : base(tableView, collection, cellKey, (float)height, initializeCellAction)
        {
            tableView.RowHeight = height;
            tableView.EstimatedRowHeight = heightHint ?? tableView.EstimatedRowHeight;
            collection.CountChanged.Select(x => x == 0).Subscribe(_isEmptySubject.OnNext);
            (collection as IReactiveCollection<TViewModel>).Do(x => _isEmptySubject.OnNext(!x.Any()));
        }

        public override void WillDisplay(UITableView tableView, UITableViewCell cell, Foundation.NSIndexPath indexPath)
        {
            if (indexPath.Section == (NumberOfSections(tableView) - 1) &&
                indexPath.Row == (RowsInSection(tableView, indexPath.Section) - 1))
            {
                // We need to skip an event loop to stay out of trouble
                BeginInvokeOnMainThread(() => _requestMoreSubject.OnNext(Unit.Default));
            }
        }

        public override void RowSelected(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            var item = ItemAt(indexPath) as ICanGoToViewModel;
            if (item != null)
                item.GoToCommand.ExecuteIfCan();

            base.RowSelected(tableView, indexPath);
        }
    }

    public interface IInformsEnd
    {
        IObservable<Unit> RequestMore { get; }
    }

    public interface IInformsEmpty
    {
        IObservable<bool> IsEmpty { get; }
    }
}

