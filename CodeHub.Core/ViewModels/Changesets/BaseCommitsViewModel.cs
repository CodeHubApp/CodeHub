using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using Octokit;
using CodeHub.Core.Services;
using System.Linq;

namespace CodeHub.Core.ViewModels.Changesets
{
    public abstract class BaseCommitsViewModel : BaseViewModel, IPaginatableViewModel, IProvidesSearchKeyword
	{
        private readonly IReactiveList<CommitItemViewModel> _commits = new ReactiveList<CommitItemViewModel>(resetChangeThreshold: 1.0);

        protected ISessionService SessionService { get; private set; }

		public string RepositoryOwner { get; private set; }

		public string RepositoryName { get; private set; }

        public IReadOnlyReactiveList<CommitItemViewModel> Commits { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

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

        protected BaseCommitsViewModel(ISessionService sessionService)
	    {
            SessionService = sessionService;
            Title = "Commits";

            Commits = _commits.CreateDerivedCollection(
                x => x, 
                x => x.Description.ContainsKeyword(SearchKeyword) || x.Name.ContainsKeyword(SearchKeyword), 
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t => {
                var ret = await RetrieveCommits();
                _commits.Reset(ret.Select(x => new CommitItemViewModel(x, GoToCommit)));
            });
	    }

        private void GoToCommit(CommitItemViewModel viewModel)
        {
            var vm = this.CreateViewModel<CommitViewModel>();
            vm.Init(RepositoryOwner, RepositoryName, viewModel.Commit.Sha, viewModel.Commit);
            NavigateTo(vm);
        }

        private async Task<IReadOnlyList<GitHubCommit>> RetrieveCommits(int page = 1)
        {
            var connection = SessionService.GitHubClient.Connection;
            var parameters = new Dictionary<string, string>();
            parameters["page"] = page.ToString();
            AddRequestParameters(parameters);
            var ret = await connection.Get<IReadOnlyList<GitHubCommit>>(RequestUri, parameters, "application/json");

            if (ret.HttpResponse.ApiInfo.Links.ContainsKey("next"))
            {
                LoadMoreCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                    var loadMore = await RetrieveCommits(page + 1);
                    _commits.AddRange(loadMore.Select(x => new CommitItemViewModel(x, GoToCommit)));
                });
            }
            else
            {
                LoadMoreCommand = null;
            }

            return ret.Body;
        }

        protected virtual void AddRequestParameters(IDictionary<string, string> parameters)
        {
        }

        protected abstract Uri RequestUri { get; }

        protected BaseCommitsViewModel Init(string repositoryOwner, string repositoryName)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            return this;
        }
	}
}

