using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.App;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.Views;
using CodeHub.iOS.Views.Issues;
using CoreGraphics;
using ReactiveUI;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Application
{
    public class FeedbackViewController : TableViewController 
    {
        private readonly UISearchBar _repositorySearchBar = new UISearchBar(new CGRect(0, 0, 320, 44));

        private readonly Lazy<UIView> _emptyView = new Lazy<UIView>((() =>
            new EmptyListView(Octicon.IssueOpened.ToEmptyListImage(), "There are no open issues.")));

        private readonly Lazy<UIView> _retryView;

        public FeedbackViewModel ViewModel { get; } = new FeedbackViewModel();

        public FeedbackViewController()
            : base(UITableViewStyle.Plain)
        {
            _retryView = new Lazy<UIView>((() =>
                new RetryListView(Octicon.IssueOpened.ToEmptyListImage(), "Error loading feedback.", LoadData)));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var tableViewSource = new FeedbackTableViewSource(TableView, ViewModel.Items);
            TableView.Source = tableViewSource;

            Appearing
              .Take(1)
              .Subscribe(_ => LoadData());

            this.WhenActivated(d =>
            {
                d(this.WhenAnyValue(x => x.ViewModel.Title)
                  .Subscribe(title => Title = title));

                d(_repositorySearchBar.GetChangedObservable()
                  .Subscribe(x => ViewModel.SearchKeyword = x));

                d(ViewModel.WhenAnyValue(x => x.IsEmpty)
                  .Where(x => x.HasValue)
                  .Select(x => x.Value)
                  .Subscribe(SetHasItems));

                d(ViewModel.Items.Changed.Select(_ => Unit.Default)
                  .StartWith(Unit.Default)
                  .Select(_ => ViewModel.Items.Select(x => x.GoToCommand.Select(__ => x)))
                  .Select(x => Observable.Merge(x))
                  .Switch()
                  .Select(x => new IssueView(x.RepositoryOwner, x.RepositoryName, x.IssueId))
                  .Subscribe(x => NavigationController.PushViewController(x, true)));
            });
        }

        private void LoadData()
        {
            if (_emptyView.IsValueCreated)
                _emptyView.Value.RemoveFromSuperview();
            if (_retryView.IsValueCreated)
                _retryView.Value.RemoveFromSuperview();
            
            ViewModel.LoadCommand.Execute()
                .Take(1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeError(setHasError);
        }

        private void setHasError(Exception error)
        {
            _retryView.Value.Alpha = 0;
            _retryView.Value.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            View.Add(_retryView.Value);
            UIView.Animate(0.8, 0, UIViewAnimationOptions.CurveEaseIn,
                           () => _retryView.Value.Alpha = 1, null);
        }

        private void SetHasItems(bool hasItems)
        {
            TableView.TableHeaderView = hasItems ? _repositorySearchBar : null;

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

