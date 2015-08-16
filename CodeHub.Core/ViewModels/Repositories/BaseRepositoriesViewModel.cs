using System;
using System.Collections.Generic;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive;
using Octokit;
using System.Threading.Tasks;
using System.Linq;

namespace CodeHub.Core.ViewModels.Repositories
{
    public abstract class BaseRepositoriesViewModel : BaseViewModel, IPaginatableViewModel, IProvidesSearchKeyword
    {
        private readonly ReactiveList<RepositoryItemViewModel> _repositoryItems = new ReactiveList<RepositoryItemViewModel>(resetChangeThreshold: 1.0);
        protected readonly ISessionService SessionService;

        public bool ShowRepositoryDescription
        {
			get { return SessionService.Account.ShowRepositoryDescriptionInList; }
        }

        public IReadOnlyReactiveList<RepositoryItemViewModel> Repositories { get; private set; }

        public bool ShowRepositoryOwner { get; protected set; }

        public IReactiveCommand<Unit> LoadCommand { get; protected set; }

        private IReactiveCommand<Unit> _loadMoreCommand;
        public IReactiveCommand<Unit> LoadMoreCommand
        {
            get { return _loadMoreCommand; }
            private set { this.RaiseAndSetIfChanged(ref _loadMoreCommand, value); }
        }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        protected BaseRepositoriesViewModel(ISessionService sessionService)
        {
            SessionService = sessionService;
            ShowRepositoryOwner = true;
            Title = "Repositories";

            Repositories = _repositoryItems.CreateDerivedCollection(
                x => x, 
                filter: x => x.Name.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t => {
                var ret = await RetrieveRepositories();
                _repositoryItems.Reset(ret.Select(x => new RepositoryItemViewModel(x, ShowRepositoryOwner, GoToRepository)));
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
                    _repositoryItems.AddRange(loadRet.Select(x => new RepositoryItemViewModel(x, ShowRepositoryOwner, GoToRepository)));
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