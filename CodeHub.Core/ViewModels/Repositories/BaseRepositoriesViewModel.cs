using System;
using System.Collections.Generic;
using CodeHub.Core.Services;
using ReactiveUI;
using Octokit;
using System.Threading.Tasks;
using System.Linq;

namespace CodeHub.Core.ViewModels.Repositories
{
    public abstract class BaseRepositoriesViewModel : BaseSearchableListViewModel<RepositoryItemViewModel, RepositoryItemViewModel>
    {
        protected readonly ISessionService SessionService;

        public bool ShowRepositoryDescription
        {
			get { return SessionService.Account.ShowRepositoryDescriptionInList; }
        }

        public bool ShowRepositoryOwner { get; protected set; }
      
        protected BaseRepositoriesViewModel(ISessionService sessionService)
        {
            SessionService = sessionService;
            ShowRepositoryOwner = true;
            Title = "Repositories";

            Items = InternalItems.CreateDerivedCollection(
                x => x, 
                filter: x => x.Name.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t => {
                var ret = await RetrieveRepositories();
                InternalItems.Reset(ret.Select(x => new RepositoryItemViewModel(x, ShowRepositoryOwner, GoToRepository)));
            });
        }

        private void GoToRepository(RepositoryItemViewModel itemViewModel)
        {
            var vm = this.CreateViewModel<RepositoryViewModel>();
            vm.Init(itemViewModel.Owner, itemViewModel.Name, itemViewModel.Repository);
            NavigateTo(vm);
        }

        private async Task<IReadOnlyList<Repository>> RetrieveRepositories(int page = 1)
        {
            var connection = SessionService.GitHubClient.Connection;
            var parameters = new Dictionary<string, string>();
            parameters["page"] = page.ToString();
            parameters["per_page"] = 75.ToString();
            var ret = await connection.Get<IReadOnlyList<Repository>>(RepositoryUri, parameters, "application/json");

            if (ret.HttpResponse.ApiInfo.Links.ContainsKey("next"))
            {
                LoadMoreCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                    var loadRet = await RetrieveRepositories(page + 1);
                    InternalItems.AddRange(loadRet.Select(x => new RepositoryItemViewModel(x, ShowRepositoryOwner, GoToRepository)));
                });
            }
            else
            {
                LoadMoreCommand = null;
            }

            return ret.Body;
        }

        protected abstract Uri RepositoryUri { get; }
    }
}