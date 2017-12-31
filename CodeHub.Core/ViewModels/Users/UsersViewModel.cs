using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using Octokit;
using ReactiveUI;
using Splat;

namespace CodeHub.Core.ViewModels.Users
{
    public class UsersViewModel : ReactiveObject
    {
        private readonly IApplicationService _applicationService;
        private readonly IAlertDialogService _dialogService;
        private readonly ReactiveList<Octokit.User> _internalItems
            = new ReactiveList<Octokit.User>(resetChangeThreshold: double.MaxValue);

        public ReactiveCommand<Unit, bool> LoadCommand { get; }

        public ReactiveCommand<Unit, bool> LoadMoreCommand { get; }

        public IReadOnlyReactiveList<UserItemViewModel> Items { get; private set; }

        public ReactiveCommand<UserItemViewModel, UserItemViewModel> ItemSelected { get; }

        private ObservableAsPropertyHelper<bool> _hasMore;
        public bool HasMore => _hasMore.Value;

        private Uri _nextPage;
        private Uri NextPage
        {
            get { return _nextPage; }
            set { this.RaiseAndSetIfChanged(ref _nextPage, value); }
        }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { this.RaiseAndSetIfChanged(ref _searchText, value); }
        }

        private readonly ObservableAsPropertyHelper<bool> _isEmpty;
        public bool IsEmpty => _isEmpty.Value;

        public static UsersViewModel CreateWatchersViewModel(string owner, string name)
            => new UsersViewModel(ApiUrls.Watchers(owner, name));

        public static UsersViewModel CreateFollowersViewModel(string user)
            => new UsersViewModel(ApiUrls.Followers(user));

        public static UsersViewModel CreateFollowingViewModel(string user)
            => new UsersViewModel(ApiUrls.Following(user));

        public static UsersViewModel CreateTeamMembersViewModel(int id)
            => new UsersViewModel(ApiUrls.TeamMembers(id));

        public static UsersViewModel CreateOrgMembersViewModel(string org)
            => new UsersViewModel(ApiUrls.Members(org));

        public static UsersViewModel CreateStargazersViewModel(string owner, string name)
            => new UsersViewModel(ApiUrls.Stargazers(owner, name));

        public static UsersViewModel CreateCollaboratorsViewModel(string owner, string name)
            => new UsersViewModel(ApiUrls.RepoCollaborators(owner, name));

        public UsersViewModel(
           Uri uri,
           IApplicationService applicationService = null,
           IAlertDialogService dialogService = null)
        {
            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IAlertDialogService>();

            NextPage = uri;

            var showDescription = _applicationService.Account.ShowRepositoryDescriptionInList;

            ItemSelected = ReactiveCommand.Create<UserItemViewModel, UserItemViewModel>(x => x);

            var userItems = _internalItems.CreateDerivedCollection(
                x => new UserItemViewModel(x, GoToUser));

            var searchUpdated = this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(400), RxApp.MainThreadScheduler);

            Items = userItems
                .CreateDerivedCollection(
                    x => x,
                    x => x.Login.ContainsKeyword(SearchText),
                    signalReset: searchUpdated);

            LoadCommand = ReactiveCommand.CreateFromTask(async t =>
            {
                _internalItems.Clear();
                var parameters = new Dictionary<string, string>();
                parameters["per_page"] = 100.ToString();
                var items = await RetrieveItems(uri, parameters);
                _internalItems.AddRange(items);
                return items.Count > 0;
            });

            var canLoadMore = this.WhenAnyValue(x => x.NextPage).Select(x => x != null);
            LoadMoreCommand = ReactiveCommand.CreateFromTask(async _ =>
            {
                var items = await RetrieveItems(NextPage);
                _internalItems.AddRange(items);
                return items.Count > 0;
            }, canLoadMore);

            LoadCommand.Select(_ => _internalItems.Count == 0)
                .ToProperty(this, x => x.IsEmpty, out _isEmpty, true);

            LoadCommand.ThrownExceptions.Subscribe(LoadingError);

            LoadMoreCommand.ThrownExceptions.Subscribe(LoadingError);

            _hasMore = this.WhenAnyValue(x => x.NextPage)
                .Select(x => x != null)
                .ToProperty(this, x => x.HasMore);
        }

        private void LoadingError(Exception err)
        {
            _dialogService.Alert("Error Loading", err.Message).ToBackground();
        }

        private void GoToUser(UserItemViewModel item)
        {
            ItemSelected.ExecuteNow(item);
        }

        private async Task<IReadOnlyList<Octokit.User>> RetrieveItems(
            Uri repositoriesUri, IDictionary<string, string> parameters = null)
        {
            try
            {
                var connection = _applicationService.GitHubClient.Connection;
                var ret = await connection.Get<IReadOnlyList<Octokit.User>>(repositoriesUri, parameters, "application/json");
                NextPage = ret.HttpResponse.ApiInfo.Links.ContainsKey("next")
                              ? ret.HttpResponse.ApiInfo.Links["next"]
                              : null;
                return ret.Body;
            }
            catch
            {
                NextPage = null;
                throw;
            }
        }
    }
}
