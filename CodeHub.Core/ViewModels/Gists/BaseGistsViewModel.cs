using System;
using ReactiveUI;
using System.Linq;
using System.Collections.Generic;
using CodeHub.Core.Services;
using System.Threading.Tasks;
using Octokit;

namespace CodeHub.Core.ViewModels.Gists
{
    public abstract class BaseGistsViewModel : BaseSearchableListViewModel<Gist, GistItemViewModel>
    {
        protected ISessionService SessionService { get; private set; }

        protected BaseGistsViewModel(ISessionService sessionService)
        {
            SessionService = sessionService;

            Items = InternalItems
                .CreateDerivedCollection(x => CreateGistItemViewModel(x))
                .CreateDerivedCollection(x => x,
                filter: x => x.Description.ContainsKeyword(SearchKeyword) || x.Title.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t => {
                InternalItems.Reset(await RetrieveGists());
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
                    InternalItems.AddRange(await RetrieveGists(page + 1));
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

