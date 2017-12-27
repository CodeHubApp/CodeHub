using System.Threading.Tasks;
using System.Collections.Generic;
using CodeHub.Core.Services;
using Splat;
using ReactiveUI;
using System.Reactive;
using System;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class CommitsViewModel : ReactiveObject
    {
        private readonly IFeaturesService _featuresService;
        private readonly IApplicationService _applicationService;

        private readonly ReactiveList<Octokit.GitHubCommit> _internalItems
            = new ReactiveList<Octokit.GitHubCommit>(resetChangeThreshold: double.MaxValue);

        public string Username { get; private set; }

        public string Repository { get; private set; }

        private bool _shouldShowPro; 
        public bool ShouldShowPro
        {
            get { return _shouldShowPro; }
            protected set { this.RaiseAndSetIfChanged(ref _shouldShowPro, value); }
        }

        public ReactiveCommand<Unit, bool> LoadCommand { get; }

        public ReactiveCommand<Unit, bool> LoadMoreCommand { get; }

        public IReadOnlyReactiveList<CommitItemViewModel> Items { get; private set; }

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

        public static CommitsViewModel CreatePullRequestCommitsViewModel(string owner, string name, int id)
            => new CommitsViewModel(Octokit.ApiUrls.PullRequestCommits(owner, name, id));

        public static CommitsViewModel CreateBranchCommitsViewModel(string owner, string name, string sha)
            => new CommitsViewModel(Octokit.ApiUrls.RepositoryCommits(owner, name)); //TODO: FIX THIS MISSING SHA

        public CommitsViewModel(
            Uri uri,
            IApplicationService applicationService = null,
            IFeaturesService featuresService = null)
        {
            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            _featuresService = featuresService ?? Locator.Current.GetService<IFeaturesService>();

            NextPage = uri;

            var showDescription = _applicationService.Account.ShowRepositoryDescriptionInList;

            var userItems = _internalItems.CreateDerivedCollection(x => new CommitItemViewModel(x));

            var searchUpdated = this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(400), RxApp.MainThreadScheduler);

            Items = userItems
                .CreateDerivedCollection(
                    x => x,
                    x => x.Title.ContainsKeyword(SearchText),
                    signalReset: searchUpdated);

            LoadCommand = ReactiveCommand.CreateFromTask(async t =>
            {
                if (_featuresService.IsProEnabled)
                    ShouldShowPro = false;
                else
                {
                    var repo = _applicationService.GitHubClient.Repository.Get(Username, Repository);
                    repo.ToBackground(x => ShouldShowPro = x.Private && !_featuresService.IsProEnabled);
                }

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
            //_dialogService.Alert("Error Loading", err.Message).ToBackground();
        }

        private async Task<IReadOnlyList<Octokit.GitHubCommit>> RetrieveItems(
            Uri repositoriesUri, IDictionary<string, string> parameters = null)
        {
            try
            {
                var connection = _applicationService.GitHubClient.Connection;
                var ret = await connection.Get<IReadOnlyList<Octokit.GitHubCommit>>(repositoriesUri, parameters, "application/json");
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

