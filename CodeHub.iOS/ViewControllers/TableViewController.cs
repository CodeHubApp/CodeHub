using System;
using UIKit;
using CoreGraphics;
using Foundation;
using CodeHub.iOS.Views;
using ReactiveUI;

namespace CodeHub.iOS.ViewControllers
{
    public class TableViewController : BaseViewController
    {
        private readonly Lazy<UITableView> _tableView;
        private UIRefreshControl _refreshControl;

        public UITableView TableView { get { return _tableView.Value; } }

        public bool ClearSelectionOnAppear { get; set; } = true;

        public virtual UIRefreshControl RefreshControl
        {
            get { return _refreshControl; }
            set
            {
                _refreshControl?.RemoveFromSuperview();
                _refreshControl = value;

                if (_refreshControl != null)
                    TableView.AddSubview(_refreshControl);
            }
        }

        public TableViewController(UITableViewStyle style)
        {
            _tableView = new Lazy<UITableView>(() => new UITableView(CGRect.Empty, style));

            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = string.Empty };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.Frame = View.Bounds;
            TableView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin;
            TableView.AutosizesSubviews = true;
            TableView.CellLayoutMarginsFollowReadableWidth = false;
            TableView.EstimatedSectionFooterHeight = 0;
            TableView.EstimatedSectionHeaderHeight = 0;
            Add(TableView);
        }

        NSObject _hideNotification, _showNotification;
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            _hideNotification = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardHideNotification);
            _showNotification = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, OnKeyboardNotification);

            var index = TableView.IndexPathForSelectedRow;
            if (ClearSelectionOnAppear && index != null)
                TableView.DeselectRow(index, true);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            View.EndEditing(true);
            
            if (_hideNotification != null)
                NSNotificationCenter.DefaultCenter.RemoveObserver(_hideNotification);
            if (_showNotification != null)
                NSNotificationCenter.DefaultCenter.RemoveObserver(_showNotification);
        }

        private void OnKeyboardHideNotification(NSNotification notification)
        {
            TableView.ContentInset = UIEdgeInsets.Zero;
            TableView.ScrollIndicatorInsets = UIEdgeInsets.Zero;
        }

        private void OnKeyboardNotification (NSNotification notification)
        {
            var keyboardFrame = UIKeyboard.FrameEndFromNotification (notification);
            var inset = new UIEdgeInsets(0, 0, keyboardFrame.Height, 0);
            TableView.ContentInset = inset;
            TableView.ScrollIndicatorInsets = inset;
        }
    }

    //public class TableViewController<T> : TableViewController
    //{
    //    private readonly UISearchBar _repositorySearchBar = new UISearchBar(new CGRect(0, 0, 320, 44));
    //    private readonly LoadingIndicatorView _loading = new LoadingIndicatorView();

    //    private readonly Lazy<UIView> _emptyView;
    //    private readonly Lazy<UIView> _retryView;
    //    private readonly Func<UITableView, UITableViewSource> _tableViewSource;

    //    public TableViewController(
    //        UITableViewStyle style,
    //        Func<UITableView, UITableViewSource> tableViewSource,
    //        Lazy<UIView> emptyView = null,
    //        Lazy<UIView> retryView = null)
    //        : base(style)
    //    {
    //        _tableViewSource = tableViewSource;
    //        _emptyView = emptyView;
    //        _retryView = retryView;
    //    }

    //    public override void ViewDidLoad()
    //    {
    //        base.ViewDidLoad();

    //        TableView.Source = _tableViewSource(TableView);

    //        Appearing
    //            .Take(1)
    //            .Subscribe(_ => LoadData());

    //        this.WhenActivated(d =>
    //        {
    //            d(_repositorySearchBar.GetChangedObservable()
    //              .Subscribe(x => ViewModel.SearchText = x));

    //            d(ViewModel.RepositoryItemSelected
    //              .Select(x => new RepositoryViewController(x.Owner, x.Name))
    //              .Subscribe(x => NavigationController.PushViewController(x, true)));

    //            d(ViewModel.WhenAnyValue(x => x.HasMore)
    //              .Subscribe(x => TableView.TableFooterView = x ? _loading : null));

    //            //d(tableViewSource.RequestMore
    //              //.InvokeCommand(ViewModel.LoadMoreCommand));

    //            d(ViewModel.LoadCommand
    //              .Select(_ => ViewModel.Items.Changed)
    //              .Switch()
    //              .Select(_ => Unit.Default)
    //              .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
    //              .Where(_ => TableView.LastItemVisible())
    //              .InvokeCommand(ViewModel.LoadMoreCommand));

    //            d(ViewModel.LoadCommand.Merge(ViewModel.LoadMoreCommand)
    //              .Select(_ => Unit.Default)
    //              .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
    //              .Where(_ => TableView.LastItemVisible())
    //              .InvokeCommand(ViewModel.LoadMoreCommand));
    //        });
    //    }

    //    private void LoadData()
    //    {
    //        if (_emptyView.IsValueCreated)
    //            _emptyView.Value.RemoveFromSuperview();
    //        if (_retryView.IsValueCreated)
    //            _retryView.Value.RemoveFromSuperview();

    //        ViewModel.LoadCommand.Execute()
    //            .Take(1)
    //            .ObserveOn(RxApp.MainThreadScheduler)
    //            .Subscribe(SetHasItems, setHasError);
    //    }

    //    private void setHasError(Exception error)
    //    {
    //        _retryView.Value.Alpha = 0;
    //        _retryView.Value.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
    //        View.Add(_retryView.Value);
    //        UIView.Animate(0.8, 0, UIViewAnimationOptions.CurveEaseIn,
    //                       () => _retryView.Value.Alpha = 1, null);
    //    }

    //    private void SetHasItems(bool hasItems)
    //    {
    //        TableView.TableHeaderView = hasItems ? _repositorySearchBar : null;

    //        if (!hasItems)
    //        {
    //            _emptyView.Value.Alpha = 0;
    //            _emptyView.Value.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
    //            View.Add(_emptyView.Value);
    //            UIView.Animate(0.8, 0, UIViewAnimationOptions.CurveEaseIn,
    //                           () => _emptyView.Value.Alpha = 1, null);
    //        }
    //    }
    //}
}

