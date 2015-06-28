using System;
using System.Reactive.Linq;
using GitHubSharp.Models;
using GitHubSharp;
using System.Collections.Generic;
using ReactiveUI;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Changesets
{
    public abstract class BaseCommitsViewModel : BaseViewModel, IPaginatableViewModel, IProvidesSearchKeyword
	{
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

        protected BaseCommitsViewModel()
	    {
            Title = "Commits";

            var gotoCommitCommand = ReactiveCommand.Create();
            gotoCommitCommand.OfType<CommitItemViewModel>().Subscribe(x => {
                var vm = this.CreateViewModel<CommitViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.Node = x.Commit.Sha;
                NavigateTo(vm);
            });

            var commits = new ReactiveList<CommitItemViewModel>();
            Commits = commits.CreateDerivedCollection(
                x => x, 
                x => x.Description.ContainsKeyword(SearchKeyword) || x.Name.ContainsKeyword(SearchKeyword), 
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                commits.SimpleCollectionLoad(CreateRequest(), 
                    x => new CommitItemViewModel(x, gotoCommitCommand.ExecuteIfCan),
                    x => LoadMoreCommand = x == null ? null : ReactiveCommand.CreateAsyncTask(_ => x())));
	    }

        protected abstract GitHubRequest<List<CommitModel>> CreateRequest();

        protected BaseCommitsViewModel Init(string repositoryOwner, string repositoryName)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            return this;
        }
	}
}

