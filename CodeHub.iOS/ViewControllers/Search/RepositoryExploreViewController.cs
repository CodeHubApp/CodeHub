using System;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using CoreGraphics;
using CodeHub.Core.ViewModels.Search;
using System.Reactive.Linq;
using CodeHub.iOS.Views;
using CodeHub.iOS.ViewControllers.Repositories;

namespace CodeHub.iOS.ViewControllers.Search
{
    public class RepositoryExploreViewController : TableViewController
    {
        private readonly UISearchBar _repositorySearchBar = new UISearchBar(new CGRect(0, 0, 320, 44));
        private readonly LoadingIndicatorView _loading = new LoadingIndicatorView();

        private readonly Lazy<UIView> emptyView = new Lazy<UIView>((() =>
            new EmptyListView(Octicon.Repo.ToEmptyListImage(), "There are no repositories.")));

        public RepositoryExploreViewModel ViewModel { get; } = new RepositoryExploreViewModel();

        public RepositoryExploreViewController()
            : base(UITableViewStyle.Plain)
        {
            Title = "Explore";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.TableHeaderView = _repositorySearchBar;
            TableView.Source = new RepositoryTableViewSource(TableView, ViewModel.Items);

            this.WhenActivated(d =>
            {
                d(_repositorySearchBar.GetChangedObservable()
                  .Subscribe(x => ViewModel.SearchText = x));
                
                d(_repositorySearchBar.GetSearchObservable()
                  .InvokeReactiveCommand(ViewModel.SearchCommand));

                d(ViewModel.SearchCommand.IsExecuting
                  .Subscribe(Searching));

                d(ViewModel.RepositoryItemSelected
                  .Select(x => new RepositoryViewController(x.Owner, x.Name, x.Repository))
                  .Subscribe(x => NavigationController.PushViewController(x, true)));

                d(ViewModel.SearchCommand.Subscribe(_ => SearchComplete()));
            });
        }

        private void SearchComplete()
        {
            if (ViewModel.Items.Count == 0)
            {
                TableView.BackgroundView = emptyView.Value;
                TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            }
            else
            {
                TableView.BackgroundView = null;
                TableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
            }
        }

        private void Searching(bool searching)
        {
            _loading.SetLoading(searching);

            if (searching)
            {
                TableView.TableFooterView = _loading;
                TableView.BackgroundView = null;
            }
            else
            {
                TableView.TableFooterView = null;
            }
        }
    }
}

