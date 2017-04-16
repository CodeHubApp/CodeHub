using System;
using UIKit;
using System.Reactive.Subjects;
using System.Reactive;
using CodeHub.Core.ViewModels;
using ReactiveUI;
using System.Reactive.Linq;
using CoreGraphics;

namespace CodeHub.iOS.TableViewSources
{
    public abstract class ReactiveTableViewSource<TViewModel> : ReactiveUI.ReactiveTableViewSource<TViewModel>, IInformsEnd
    {
        private readonly Subject<Unit> _requestMoreSubject = new Subject<Unit>();
        private readonly Subject<CGPoint> _scrollSubject = new Subject<CGPoint>();

        public IObservable<CGPoint> DidScroll
        {
            get { return _scrollSubject.AsObservable(); }
        }

        public IObservable<Unit> RequestMore
        {
            get { return _requestMoreSubject; }
        }

        public override void Scrolled(UIScrollView scrollView)
        {
            _scrollSubject.OnNext(scrollView.ContentOffset);
        }

        ~ReactiveTableViewSource()
        {
            Console.WriteLine("Destorying " + GetType().Name);
        }

        protected ReactiveTableViewSource(UITableView tableView, nfloat height, nfloat? heightHint = null)
            : base(tableView)
        {
            tableView.RowHeight = height;
            tableView.EstimatedRowHeight = heightHint ?? tableView.EstimatedRowHeight;
        }

        protected ReactiveTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<TViewModel> collection,
            Foundation.NSString cellKey, nfloat height, nfloat? heightHint = null, Action<UITableViewCell> initializeCellAction = null)
            : base(tableView, collection, cellKey, (float)height, initializeCellAction)
        {
            tableView.RowHeight = height;
            tableView.EstimatedRowHeight = heightHint ?? tableView.EstimatedRowHeight;
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
            item?.GoToCommand.ExecuteNow();

            base.RowSelected(tableView, indexPath);
        }

        protected override void Dispose(bool disposing)
        {
            _requestMoreSubject.Dispose();
            _scrollSubject.Dispose();
            base.Dispose(disposing);
        }
    }

    public interface IInformsEnd
    {
        IObservable<Unit> RequestMore { get; }
    }
}

