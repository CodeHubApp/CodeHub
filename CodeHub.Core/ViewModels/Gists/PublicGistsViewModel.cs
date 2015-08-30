using System;
using ReactiveUI;
using System.Linq;
using CodeHub.Core.Services;
using Octokit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Gists
{
    public class PublicGistsViewModel : BaseListViewModel<Gist, GistItemViewModel>
    {
        private readonly ISessionService _sessionService;

        public PublicGistsViewModel(ISessionService sessionService)
        {
            _sessionService = sessionService;

            Title = "Public Gists";

            Items = InternalItems.CreateDerivedCollection(x => CreateGistItemViewModel(x));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t => {
                InternalItems.Reset(await RetrieveGists());
            });
        }

        private GistItemViewModel CreateGistItemViewModel(Gist gist)
        {
            var title = (gist.Owner == null) ? "Anonymous" : gist.Owner.Login;
            var description = string.IsNullOrEmpty(gist.Description) ? "Gist " + gist.Id : gist.Description;
            var imageUrl = (gist.Owner == null) ? null : gist.Owner.AvatarUrl;
            if (gist.Files.Count > 0)
                title = gist.Files.First().Key;

            return new GistItemViewModel(title, imageUrl, description, gist.UpdatedAt, _ => {
                var vm = this.CreateViewModel<GistViewModel>();
                vm.Init(gist.Id, gist);
                NavigateTo(vm);
            });
        }

        private async Task<IReadOnlyList<Gist>> RetrieveGists(int page = 1)
        {
            var connection = _sessionService.GitHubClient.Connection;
            var parameters = new Dictionary<string, string>();
            parameters["page"] = page.ToString();
            var ret = await connection.Get<IReadOnlyList<Gist>>(ApiUrls.PublicGists(), parameters, "application/json");

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
    }
}
