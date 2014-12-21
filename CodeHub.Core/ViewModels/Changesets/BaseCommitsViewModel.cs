using System;
using System.Reactive.Linq;
using GitHubSharp.Models;
using GitHubSharp;
using System.Collections.Generic;
using ReactiveUI;
using Xamarin.Utilities.ViewModels;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Changesets
{
    public abstract class BaseCommitsViewModel : BaseViewModel, IPaginatableViewModel, IProvidesSearchKeyword
	{
		public string RepositoryOwner { get; set; }

		public string RepositoryName { get; set; }

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
            gotoCommitCommand.OfType<CommitItemViewModel>().Subscribe(x =>
            {
                var vm = this.CreateViewModel<CommitViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.Node = x.Commit.Sha;
                NavigateTo(vm);
            });

            var commits = new ReactiveList<CommitModel>();
            Commits = commits.CreateDerivedCollection(
                x => new CommitItemViewModel(x, gotoCommitCommand.ExecuteIfCan), 
                ContainsSearchKeyword, 
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                commits.SimpleCollectionLoad(CreateRequest(), t as bool?, 
                    x => LoadMoreCommand = x == null ? null : ReactiveCommand.CreateAsyncTask(_ => x())));
	    }

        protected abstract GitHubRequest<List<CommitModel>> CreateRequest();

        private bool ContainsSearchKeyword(CommitModel x)
        {
            try
            {
                if (x == null || x.Commit == null || x.Commit.Message == null)
                    return false;
                return x.Commit.Message.ContainsKeyword(SearchKeyword) || x.GenerateCommiterName().ContainsKeyword(SearchKeyword);
            }
            catch
            {
                return false;
            }
        }
	}
}

