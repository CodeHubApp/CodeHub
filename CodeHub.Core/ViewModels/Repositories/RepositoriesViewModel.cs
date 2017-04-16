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
        private readonly ReactiveList<Repository> _internalItems = new ReactiveList<Repository>(resetChangeThreshold: double.MaxValue);

        public ReactiveCommand<Unit, Unit> LoadCommand { get; }

        public ReactiveCommand<Unit, Unit> LoadMoreCommand { get; }

        public IReadOnlyReactiveList<RepositoryItemViewModel> Items { get; private set; }

        public ReactiveCommand<RepositoryItemViewModel, RepositoryItemViewModel> RepositoryItemSelected { get; }

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
            => new RepositoriesViewModel(ApiUrls.Repositories(), false);

        public static RepositoriesViewModel CreateUsersViewModel(string username)
        {
            var applicationService = Locator.Current.GetService<IApplicationService>();
            var isCurrent = string.Equals(applicationService.Account.Username, username, StringComparison.OrdinalIgnoreCase);
            return new RepositoriesViewModel(isCurrent ? ApiUrls.Repositories() : ApiUrls.Repositories(username));
        }

        public RepositoriesViewModel(
            Uri repositoriesUri,
            bool showOwner = true,
            IApplicationService applicationService = null,
            IAlertDialogService dialogService = null)
        {
            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IAlertDialogService>();

            var showDescription = _applicationService.Account.ShowRepositoryDescriptionInList;

            RepositoryItemSelected = ReactiveCommand.Create<RepositoryItemViewModel, RepositoryItemViewModel>(x => x);

            Items = _internalItems
                .CreateDerivedCollection(
                    x => new RepositoryItemViewModel(x, showOwner, showDescription, GoToRepository),
                    x => x.Name.ContainsKeyword(SearchText),
                    signalReset: this.WhenAnyValue(x => x.SearchText));

            LoadCommand = ReactiveCommand.CreateFromTask(async t =>
            {
                _internalItems.Clear();
                var parameters = new Dictionary<string, string>();
                parameters["per_page"] = 75.ToString();
                var items = await RetrieveRepositories(repositoriesUri, parameters);
                _internalItems.AddRange(items);
            });

            var canLoadMore = this.WhenAnyValue(x => x.NextPage).Select(x => x != null);
            LoadMoreCommand = ReactiveCommand.CreateFromTask(async _ =>
            {
                var items = await RetrieveRepositories(NextPage);
                _internalItems.AddRange(items);
            }, canLoadMore);

            LoadCommand.Select(_ => _internalItems.Count == 0)
                .ToProperty(this, x => x.IsEmpty, out _isEmpty, true);

            LoadCommand.ThrownExceptions.Subscribe(LoadingError);

            LoadMoreCommand.ThrownExceptions.Subscribe(LoadingError);
        }

        private void LoadingError(Exception err)
        {
            _dialogService.Alert("Error Loading", err.Message).ToBackground();
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