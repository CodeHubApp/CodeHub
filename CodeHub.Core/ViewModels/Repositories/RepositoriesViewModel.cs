using ReactiveUI;
using System.Reactive;
using CodeHub.Core.Services;
using Octokit;
using Splat;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesViewModel : ReactiveObject
    {
        private readonly IApplicationService _applicationService;
        private readonly IAlertDialogService _dialogService;
        private readonly ReactiveList<Repository> _internalItems
            = new ReactiveList<Repository>(resetChangeThreshold: double.MaxValue);

        public ReactiveCommand<Unit, bool> LoadCommand { get; }

        public ReactiveCommand<Unit, bool> LoadMoreCommand { get; }

        public IReadOnlyReactiveList<RepositoryItemViewModel> Items { get; private set; }

        public ReactiveCommand<RepositoryItemViewModel, RepositoryItemViewModel> RepositoryItemSelected { get; }
        
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

        public static RepositoriesViewModel CreateWatchedViewModel()
            => new RepositoriesViewModel(ApiUrls.Watched());

        public static RepositoriesViewModel CreateStarredViewModel()
            => new RepositoriesViewModel(ApiUrls.Starred());

        public static RepositoriesViewModel CreateForkedViewModel(string username, string repository)
            => new RepositoriesViewModel(ApiUrls.RepositoryForks(username, repository));

        public static RepositoriesViewModel CreateOrganizationViewModel(string org)
            => new RepositoriesViewModel(ApiUrls.OrganizationRepositories(org));

        public static RepositoriesViewModel CreateTeamViewModel(int id)
            => new RepositoriesViewModel(ApiUrls.TeamRepositories(id));

        public static RepositoriesViewModel CreateMineViewModel()
            => new RepositoriesViewModel(ApiUrls.Repositories(), false, "owner,collaborator");

        public static RepositoriesViewModel CreateUsersViewModel(string username)
        {
            var applicationService = Locator.Current.GetService<IApplicationService>();
            var isCurrent = string.Equals(applicationService.Account.Username, username, StringComparison.OrdinalIgnoreCase);

            return isCurrent
                ? CreateMineViewModel()
                : new RepositoriesViewModel(ApiUrls.Repositories(username));
        }

        public RepositoriesViewModel(
            Uri repositoriesUri,
            bool showOwner = true,
            string affiliation = null,
            IApplicationService applicationService = null,
            IAlertDialogService dialogService = null)
        {
            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IAlertDialogService>();

            NextPage = repositoriesUri;

            var showDescription = _applicationService.Account.ShowRepositoryDescriptionInList;

            RepositoryItemSelected = ReactiveCommand.Create<RepositoryItemViewModel, RepositoryItemViewModel>(x => x);

            var repositoryItems = _internalItems.CreateDerivedCollection(
                x => new RepositoryItemViewModel(x, showOwner, showDescription, GoToRepository));

            var searchUpdated = this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(400), RxApp.MainThreadScheduler);

            Items = repositoryItems
                .CreateDerivedCollection(
                    x => x,
                    x => x.Name.ContainsKeyword(SearchText),
                    signalReset: searchUpdated);

            LoadCommand = ReactiveCommand.CreateFromTask(async t =>
            {
                _internalItems.Clear();
                var parameters = new Dictionary<string, string>
                {
                    ["per_page"] = 75.ToString()
                };

                if (affiliation != null)
                {
                    parameters["affiliation"] = affiliation;
                }

                var items = await RetrieveRepositories(repositoriesUri, parameters);
                _internalItems.AddRange(items);
                return items.Count > 0;
            });

            var canLoadMore = this.WhenAnyValue(x => x.NextPage).Select(x => x != null);
            LoadMoreCommand = ReactiveCommand.CreateFromTask(async _ =>
            {
                var items = await RetrieveRepositories(NextPage);
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
            var message = err.Message;
            var baseException = err.GetInnerException();
            if (baseException is System.Net.Sockets.SocketException)
            {
                message = "Unable to communicate with GitHub. " + baseException.Message;
            }

            _dialogService.Alert("Error Loading", message).ToBackground();
        }

        private void GoToRepository(RepositoryItemViewModel item)
        {
            RepositoryItemSelected.ExecuteNow(item);
        }

        private async Task<IReadOnlyList<Repository>> RetrieveRepositories(
            Uri repositoriesUri, IDictionary<string, string> parameters = null)
        {
            var connection = _applicationService.GitHubClient.Connection;
            var ret = await connection.Get<IReadOnlyList<Repository>>(repositoriesUri, parameters, "application/json");
            NextPage = ret.HttpResponse.ApiInfo.Links.ContainsKey("next")
                          ? ret.HttpResponse.ApiInfo.Links["next"]
                          : null;
            return ret.Body;
        }
    }
}