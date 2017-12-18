using System;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using CoreGraphics;
using System.Reactive.Linq;
using CodeHub.iOS.Views;
using CodeHub.Core.ViewModels.Repositories;
using System.Reactive;

namespace CodeHub.iOS.ViewControllers.Repositories
{
	public class RepositoriesViewController : TableViewController
	{
		private readonly UISearchBar _repositorySearchBar = new UISearchBar(new CGRect(0, 0, 320, 44));
		private readonly LoadingIndicatorView _loading = new LoadingIndicatorView();

        private readonly Lazy<UIView> _emptyView = new Lazy<UIView>((() =>
            new EmptyListView(Octicon.Repo.ToEmptyListImage(), "There are no repositories.")));

        private readonly Lazy<UIView> _retryView;

        public RepositoriesViewModel ViewModel { get; }

        public static RepositoriesViewController CreateMineViewController()
        {
            var viewModel = RepositoriesViewModel.CreateMineViewModel();
            return new RepositoriesViewController(viewModel);
        }

        public static RepositoriesViewController CreateUserViewController(string username)
        {
            var viewModel = RepositoriesViewModel.CreateUsersViewModel(username);
            return new RepositoriesViewController(viewModel);
        }

        public static RepositoriesViewController CreateStarredViewController()
        {
            var viewModel = RepositoriesViewModel.CreateStarredViewModel();
            return new RepositoriesViewController(viewModel) { Title = "Starred" };
        }

        public static RepositoriesViewController CreateWatchedViewController()
        {
            var viewModel = RepositoriesViewModel.CreateWatchedViewModel();
            return new RepositoriesViewController(viewModel) { Title = "Watched" };
        }

        public static RepositoriesViewController CreateForkedViewController(string username, string repository)
        {
            var viewModel = RepositoriesViewModel.CreateForkedViewModel(username, repository);
            return new RepositoriesViewController(viewModel) { Title = "Forks" };
        }

        public static RepositoriesViewController CreateOrganizationViewController(string org)
        {
            var viewModel = RepositoriesViewModel.CreateOrganizationViewModel(org);
            return new RepositoriesViewController(viewModel);
        }

        public RepositoriesViewController(RepositoriesViewModel viewModel)
			: base(UITableViewStyle.Plain)
		{
            ViewModel = viewModel;
            Title = "Repositories";

            _retryView = new Lazy<UIView>((() =>
                new RetryListView(Octicon.Repo.ToEmptyListImage(), "Error loading repositories.", LoadData)));
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            var tableViewSource = new RepositoryTableViewSource(TableView, ViewModel.Items);
            TableView.Source = tableViewSource;

            Appearing
                .Take(1)
                .Subscribe(_ => LoadData());

			this.WhenActivated(d =>
			{
				d(_repositorySearchBar.GetChangedObservable()
				  .Subscribe(x => ViewModel.SearchText = x));

				d(ViewModel.RepositoryItemSelected
                  .Select(x => new RepositoryViewController(x.Owner, x.Name, x.Repository))
				  .Subscribe(x => NavigationController.PushViewController(x, true)));

                d(ViewModel.WhenAnyValue(x => x.HasMore)
                  .Subscribe(x => TableView.TableFooterView = x ? _loading : null));

                d(tableViewSource.RequestMore
                  .InvokeReactiveCommand(ViewModel.LoadMoreCommand));

                d(ViewModel.LoadCommand
                  .Select(_ => ViewModel.Items.Changed)
                  .Switch()
                  .Select(_ => Unit.Default)
                  .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
                  .Where(_ => TableView.LastItemVisible())
                  .InvokeReactiveCommand(ViewModel.LoadMoreCommand));

                d(ViewModel.LoadCommand.Merge(ViewModel.LoadMoreCommand)
                  .Select(_ => Unit.Default)
                  .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
                  .Where(_ => TableView.LastItemVisible())
                  .InvokeReactiveCommand(ViewModel.LoadMoreCommand));
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
                .Subscribe(SetHasItems, setHasError);
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

