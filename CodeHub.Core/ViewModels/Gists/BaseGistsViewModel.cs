using System;
using ReactiveUI;
using System.Linq;
using System.Collections.Generic;
using System.Reactive;
using CodeHub.Core.Services;
using System.Threading.Tasks;
using Octokit;

namespace CodeHub.Core.ViewModels.Gists
{
    public interface IGistsViewModel
    {
        IReadOnlyReactiveList<GistItemViewModel> Gists { get; }
    }

    public abstract class BaseGistsViewModel : BaseViewModel, IProvidesSearchKeyword, ILoadableViewModel, IPaginatableViewModel, IGistsViewModel
    {
        protected ReactiveList<Gist> InternalGists = new ReactiveList<Gist>(resetChangeThreshold: 1.0);
        public IReadOnlyReactiveList<GistItemViewModel> Gists { get; private set; }

        protected ISessionService SessionService { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        private IReactiveCommand<Unit> _loadMoreCommand;
        public IReactiveCommand<Unit> LoadMoreCommand
        {
            get { return _loadMoreCommand; }
            private set { this.RaiseAndSetIfChanged(ref _loadMoreCommand, value); }
        }

        protected BaseGistsViewModel(ISessionService sessionService)
        {
            SessionService = sessionService;

            Gists = InternalGists
                .CreateDerivedCollection(x => CreateGistItemViewModel(x))
                .CreateDerivedCollection(x => x,
                filter: x => x.Description.ContainsKeyword(SearchKeyword) || x.Title.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t => {
                InternalGists.Reset(await RetrieveGists());
            });
        }

        private GistItemViewModel CreateGistItemViewModel(Gist gist)
        {
            var title = GetGistTitle(gist);
            var description = string.IsNullOrEmpty(gist.Description) ? "Gist " + gist.Id : gist.Description;
            var imageUrl = (gist.Owner == null) ? null : gist.Owner.AvatarUrl;

            return new GistItemViewModel(title, imageUrl, description, gist.UpdatedAt, _ => {
                var vm = this.CreateViewModel<GistViewModel>();
                vm.Init(gist.Id, gist);
                NavigateTo(vm);
            });
        }

        private static string GetGistTitle(Gist gist)
        {
            var title = (gist.Owner == null) ? "Anonymous" : gist.Owner.Login;
            if (gist.Files.Count > 0)
                title = gist.Files.First().Key;
            return title;
        }

        private async Task<IReadOnlyList<Gist>> RetrieveGists(int page = 1)
        {
            var connection = SessionService.GitHubClient.Connection;
            var parameters = new Dictionary<string, string>();
            parameters["page"] = page.ToString();
            parameters["per_page"] = 100.ToString();
            var ret = await connection.Get<IReadOnlyList<Gist>>(RequestUri, parameters, "application/json");

            if (ret.HttpResponse.ApiInfo.Links.ContainsKey("next"))
            {
                LoadMoreCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                    InternalGists.AddRange(await RetrieveGists(page + 1));
                });
            }
            else
            {
                LoadMoreCommand = null;
            }

            return ret.Body;
        }

        protected abstract Uri RequestUri { get; }
    }
}

