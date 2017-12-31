using System;
using System.Reactive;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Users;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.Views;
using CoreGraphics;
using ReactiveUI;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Users
{
    public class UsersViewController : TableViewController
    {
        private readonly UISearchBar _repositorySearchBar = new UISearchBar(new CGRect(0, 0, 320, 44));
        private readonly LoadingIndicatorView _loading = new LoadingIndicatorView();

        private readonly Lazy<UIView> emptyView = new Lazy<UIView>((() =>
            new EmptyListView(Octicon.Person.ToEmptyListImage(), "There are no users.")));

        public UsersViewModel ViewModel { get; }

        public static UsersViewController CreateWatchersViewController(string owner, string name)
        {
            var viewModel = UsersViewModel.CreateWatchersViewModel(owner, name);
            return new UsersViewController(viewModel) { Title = "Watchers" };
        }

        public static UsersViewController CreateFollowersViewController(string username)
        {
            var viewModel = UsersViewModel.CreateFollowersViewModel(username);
            return new UsersViewController(viewModel) { Title = "Followers" };
        }

        public static UsersViewController CreateFollowingViewController(string username)
        {
            var viewModel = UsersViewModel.CreateFollowingViewModel(username);
            return new UsersViewController(viewModel) { Title = "Following" };
        }

        public static UsersViewController CreateOrganizationMembersViewController(string organization)
        {
            var viewModel = UsersViewModel.CreateOrgMembersViewModel(organization);
            return new UsersViewController(viewModel) { Title = "Members" };
        }

        public static UsersViewController CreateStargazersViewController(string username, string repository)
        {
            var viewModel = UsersViewModel.CreateStargazersViewModel(username, repository);
            return new UsersViewController(viewModel) { Title = "Stargazers" };
        }

        public static UsersViewController CreateTeamMembersViewController(int id)
        {
            var viewModel = UsersViewModel.CreateTeamMembersViewModel(id);
            return new UsersViewController(viewModel) { Title = "Members" };
        }

        public static UsersViewController CreateCollaboratorsViewController(string username, string repository)
        {
            var viewModel = UsersViewModel.CreateCollaboratorsViewModel(username, repository);
            return new UsersViewController(viewModel) { Title = "Collaborators" };
        }

        public UsersViewController(UsersViewModel viewModel)
            : base(UITableViewStyle.Plain)
        {
            ViewModel = viewModel;
            Title = "Users";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var tableViewSource = new UserTableViewSource(TableView, ViewModel.Items);
            TableView.Source = tableViewSource;

            Appearing
                .Take(1)
                .Select(_ => ViewModel.LoadCommand.Execute())
                .Switch()
                .Take(1)
                .Catch(Observable.Return(false))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(SetItemsPresent);

            this.WhenActivated(d =>
            {
                d(_repositorySearchBar.GetChangedObservable()
                  .Subscribe(x => ViewModel.SearchText = x));

                d(ViewModel.ItemSelected
                  .Select(x => new UserViewController(x.User))
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

        private void SetItemsPresent(bool hasItems)
        {
            TableView.TableHeaderView = hasItems ? _repositorySearchBar : null;
            TableView.SeparatorStyle = hasItems 
                ? UITableViewCellSeparatorStyle.SingleLine 
                : UITableViewCellSeparatorStyle.None;

            if (hasItems)
            {
                TableView.BackgroundView = null;
            }
            else
            {
                emptyView.Value.Alpha = 0;
                TableView.BackgroundView = emptyView.Value;
                UIView.Animate(0.8, 0, UIViewAnimationOptions.CurveEaseIn,
                               () => emptyView.Value.Alpha = 1, null);
            }
        }
    }
}
