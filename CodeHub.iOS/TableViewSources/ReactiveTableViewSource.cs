using System;
using UIKit;
using System.Reactive.Subjects;
using System.Reactive;
using CodeHub.Core.ViewModels;
using ReactiveUI;

namespace CodeHub.iOS.TableViewSources
{
    public abstract class ReactiveTableViewSource<TViewModel> : ReactiveUI.ReactiveTableViewSource<TViewModel>, IInformsEnd
    {
        private readonly ISubject<Unit> _requestMoreSubject = new Subject<Unit>();
        private readonly nfloat _estimatedHeight;

        public IObservable<Unit> RequestMore
        {
            get { return _requestMoreSubject; }
        }

        protected ReactiveTableViewSource(UITableView tableView, nfloat? sizeHint = null)
            : base(tableView)
        {
            _estimatedHeight = sizeHint ?? UITableView.AutomaticDimension;
        }

        protected ReactiveTableViewSource(UITableView tableView, ReactiveUI.IReactiveNotifyCollectionChanged<TViewModel> collection, 
            Foundation.NSString cellKey, nfloat sizeHint, Action<UITableViewCell> initializeCellAction = null) 
            : base(tableView, collection, cellKey, (float)sizeHint, initializeCellAction)
        {
            _estimatedHeight = sizeHint;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            if (Data == null || Data.Count == 0)
                return 0;
            return base.RowsInSection(tableview, section);
        }

        public override string TitleForFooter(UITableView tableView, nint section)
        {
            if (Data == null || Data.Count == 0)
                return string.Empty;
            return base.TitleForFooter(tableView, section);
        }

        public override string TitleForHeader(UITableView tableView, nint section)
        {
            if (Data == null || Data.Count == 0)
                return string.Empty;
            return base.TitleForHeader(tableView, section);
        }

        public override nfloat GetHeightForHeader(UITableView tableView, nint section)
        {
            if (Data == null || Data.Count == 0)
                return UITableView.AutomaticDimension;
            return base.GetHeightForHeader(tableView, section);
        }

        public override nfloat GetHeightForFooter(UITableView tableView, nint section)
        {
            if (Data == null || Data.Count == 0)
                return UITableView.AutomaticDimension;
            return base.GetHeightForFooter(tableView, section);
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

        public override nfloat EstimatedHeight(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            return _estimatedHeight;
        }

        public override void RowSelected(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);

            var item = ItemAt(indexPath) as ICanGoToViewModel;
            if (item != null)
                item.GoToCommand.ExecuteIfCan();

            tableView.DeselectRow(indexPath, true);
        }
    }

    public interface IInformsEnd
    {
        IObservable<Unit> RequestMore { get; }
    }
}

