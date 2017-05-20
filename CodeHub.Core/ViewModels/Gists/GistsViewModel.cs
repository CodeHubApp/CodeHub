using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUI;
using CodeHub.Core.Services;
using System.Reactive;
using System;
using Octokit;
using System.Reactive.Linq;
using Splat;
using CodeHub.Core.Messages;

namespace CodeHub.Core.ViewModels.Gists
{
    public class CurrentUserGistsViewModel : GistsViewModel
    {
        private readonly IDisposable _addToken;

        public CurrentUserGistsViewModel(string username, IMessageService messageService = null)
            : base(ApiUrls.UsersGists(username))
        {
            messageService = messageService ?? Locator.Current.GetService<IMessageService>();
            _addToken = messageService.Listen<GistAddMessage>(msg => Gists.Insert(0, msg.Gist));
        }
    }

    public class GistsViewModel : ReactiveObject
    {
        private readonly IApplicationService _applicationService;
        private readonly IAlertDialogService _dialogService;
        private readonly ReactiveList<Gist> _internalItems
            = new ReactiveList<Gist>(resetChangeThreshold: double.MaxValue);

        public ReactiveCommand<Unit, bool> LoadCommand { get; }

        public ReactiveCommand<Unit, bool> LoadMoreCommand { get; }

        public IReadOnlyReactiveList<GistItemViewModel> Items { get; private set; }

        protected ReactiveList<Gist> Gists => _internalItems;

        public ReactiveCommand<GistItemViewModel, GistItemViewModel> ItemSelected { get; }

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

        public static GistsViewModel CreatePublicGistsViewModel()
            => new GistsViewModel(ApiUrls.PublicGists());

        public static GistsViewModel CreateStarredGistsViewModel()
            => new GistsViewModel(ApiUrls.StarredGists());

        public static GistsViewModel CreateUserGistsViewModel(string username)
            => new GistsViewModel(ApiUrls.UsersGists(username));

        public GistsViewModel(
           Uri uri,
           IApplicationService applicationService = null,
           IAlertDialogService dialogService = null)
        {
            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IAlertDialogService>();

            NextPage = uri;

            var showDescription = _applicationService.Account.ShowRepositoryDescriptionInList;

            ItemSelected = ReactiveCommand.Create<GistItemViewModel, GistItemViewModel>(x => x);

            var gistItems = _internalItems.CreateDerivedCollection(
                x => new GistItemViewModel(x, GoToItem));

            var searchUpdated = this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(400), RxApp.MainThreadScheduler);

            Items = gistItems.CreateDerivedCollection(
                x => x,
                x => x.Title.ContainsKeyword(SearchText) || x.Description.ContainsKeyword(SearchText),
                signalReset: searchUpdated);

            LoadCommand = ReactiveCommand.CreateFromTask(async t =>
            {
                _internalItems.Clear();
                var parameters = new Dictionary<string, string> { ["per_page"] = 100.ToString() };
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

        private void GoToItem(GistItemViewModel item)
        {
            ItemSelected.ExecuteNow(item);
        }

        private async Task<IReadOnlyList<Gist>> RetrieveItems(
            Uri repositoriesUri, IDictionary<string, string> parameters = null)
        {
            try
            {
                var connection = _applicationService.GitHubClient.Connection;
                var ret = await connection.Get<IReadOnlyList<Gist>>(repositoriesUri, parameters, "application/json");
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

