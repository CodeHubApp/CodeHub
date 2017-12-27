using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using CodeHub.Core.Utils;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.Views;
using CoreGraphics;
using ReactiveUI;
using UIKit;

namespace CodeHub.iOS.ViewControllers
{
    public class TableListViewController<T> : TableViewController
    {
        private readonly LoadingIndicatorView _loading = new LoadingIndicatorView();
        private readonly Section _section = new Section();
        private readonly UISearchBar _searchBar = new UISearchBar(new CGRect(0, 0, 320, 44));
        private readonly Lazy<UIView> _emptyView;
        private readonly Lazy<UIView> _retryView;
        private IPaginator<T> _paginator;

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        private bool _hasMore;
        private bool HasMore
        {
            get { return _hasMore; }
            set { this.RaiseAndSetIfChanged(ref _hasMore, value); }
        }

        public ReactiveCommand<Unit, Unit> LoadCommand { get; }

        public ReactiveCommand<Unit, Unit> LoadMoreCommand { get; }

        public TableListViewController(
            IPaginator<T> paginator,
            Func<T, Element> itemToElement,
            Func<UIImage> listImage)
            : base(UITableViewStyle.Plain)
        {
            _paginator = paginator;

            _retryView = new Lazy<UIView>((() =>
               new RetryListView(listImage(),  "Error loading data.", LoadData)));

            _emptyView = new Lazy<UIView>((() =>
               new EmptyListView(listImage(), "There are no items.")));

            LoadCommand = ReactiveCommand.CreateFromTask(async t =>
            {
                _section.Clear();
                _paginator.Reset();
                var items = await _paginator.Next();
                _section.Reset(items.Select(itemToElement));
                HasMore = _paginator.HasMore;
            });

            LoadMoreCommand = ReactiveCommand.CreateFromTask(async t =>
            {
                var items = await _paginator.Next();
                _section.Add(items.Select(itemToElement));
                HasMore = _paginator.HasMore;
            }, this.WhenAnyValue(x => x.HasMore));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var tableSource = new DialogTableViewSource(TableView);
            tableSource.Root.Add(_section);
            TableView.Source = tableSource;

            Appearing
                .Take(1)
                .Do(_ => LoadData())
                .Subscribe();

            this.WhenActivated(d =>
            {
                d(_searchBar.GetChangedObservable()
                  .Subscribe(x => SearchKeyword = x));

                d(this.WhenAnyValue(x => x.SearchKeyword)
                  .Subscribe(x => _searchBar.Text = x));

                d(this.WhenAnyValue(x => x.HasMore)
                  .Subscribe(x => TableView.TableFooterView = x ? _loading : null));

                d(tableSource.RequestMoreObservable
                  .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
                  .InvokeReactiveCommand(LoadMoreCommand));

                d(LoadCommand.Merge(LoadMoreCommand)
                  .Select(_ => Unit.Default)
                  .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
                  .Where(_ => TableView.LastItemVisible())
                  .InvokeReactiveCommand(LoadMoreCommand));
            });
        }

        private void LoadData()
        {
            if (_emptyView.IsValueCreated)
                _emptyView.Value.RemoveFromSuperview();
            if (_retryView.IsValueCreated)
                _retryView.Value.RemoveFromSuperview();

            LoadCommand.Execute()
                .Take(1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeError(SetHasError);
        }

        private void SetHasError(Exception error)
        {
            _retryView.Value.Alpha = 0;
            _retryView.Value.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            View.Add(_retryView.Value);
            UIView.Animate(0.8, 0, UIViewAnimationOptions.CurveEaseIn,
                           () => _retryView.Value.Alpha = 1, null);
        }

        private void SetHasItems(bool hasItems)
        {
            TableView.TableHeaderView = hasItems ? _searchBar : null;

            if (!hasItems)
            {
                _emptyView.Value.Alpha = 0;
                _emptyView.Value.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                View.Add(_emptyView.Value);
                UIView.Animate(0.8, 0, UIViewAnimationOptions.CurveEaseIn,
                               () => _emptyView.Value.Alpha = 1, null);
            }
        }
    }
}
