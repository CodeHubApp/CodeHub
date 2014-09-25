using System;
using System.Reactive.Linq;
using GitHubSharp.Models;
using GitHubSharp;
using System.Collections.Generic;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace CodeHub.Core.ViewModels.Changesets
{
    public abstract class CommitsViewModel : BaseViewModel, ILoadableViewModel
	{
		public string RepositoryOwner { get; set; }

		public string RepositoryName { get; set; }

        public IReactiveCommand<object> GoToChangesetCommand { get; private set; }

        public IReadOnlyReactiveList<CommitItemViewModel> Commits { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

	    protected CommitsViewModel()
	    {
            var commits = new ReactiveList<CommitModel>();
            Commits = commits.CreateDerivedCollection(
                x => new CommitItemViewModel(x, GoToChangesetCommand.ExecuteIfCan), 
                ContainsSearchKeyword, 
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            GoToChangesetCommand = ReactiveCommand.Create();
            GoToChangesetCommand.OfType<CommitItemViewModel>().Subscribe(x =>
	        {
	            var vm = CreateViewModel<ChangesetViewModel>();
	            vm.RepositoryOwner = RepositoryOwner;
	            vm.RepositoryName = RepositoryName;
                vm.Node = x.Commit.Sha;
                ShowViewModel(vm);
	        });

            LoadCommand = ReactiveCommand.CreateAsyncTask(x => commits.SimpleCollectionLoad(GetRequest(), x as bool?));
	    }

        protected abstract GitHubRequest<List<CommitModel>> GetRequest();

        private bool ContainsSearchKeyword(CommitModel x)
        {
            try
            {
                return x.Commit.Message.ContainsKeyword(SearchKeyword) || x.GenerateCommiterName().ContainsKeyword(SearchKeyword);
            }
            catch
            {
                return false;
            }
        }
	}
}

