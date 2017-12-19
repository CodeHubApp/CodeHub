using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Search;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.ViewControllers.Users;
using CodeHub.iOS.Views;
using CoreGraphics;
using ReactiveUI;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Search
{
    public class UserExploreViewController : TableViewController
    {
        private readonly UISearchBar _searchBar = new UISearchBar(new CGRect(0, 0, 320, 44));
        private readonly LoadingIndicatorView _loading = new LoadingIndicatorView();

        private readonly Lazy<UIView> emptyView = new Lazy<UIView>((() =>
            new EmptyListView(Octicon.Person.ToEmptyListImage(), "There are no users.")));

        public UserExploreViewModel ViewModel { get; } = new UserExploreViewModel();

        public UserExploreViewController()
            : base(UITableViewStyle.Plain)
        {
            Title = "Users";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.TableHeaderView = _searchBar;
            TableView.Source = new UserTableViewSource(TableView, ViewModel.Items);

            this.WhenActivated(d =>
            {
                d(_searchBar.GetChangedObservable()
                  .Subscribe(x => ViewModel.SearchText = x));

                d(_searchBar.GetSearchObservable()
                  .InvokeReactiveCommand(ViewModel.SearchCommand));

                d(ViewModel.SearchCommand.IsExecuting
                  .Subscribe(Searching));

                d(ViewModel.RepositoryItemSelected
                  .Select(x => new UserViewController(x.User))
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
