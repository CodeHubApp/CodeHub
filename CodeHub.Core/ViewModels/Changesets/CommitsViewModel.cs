using System;
using System.Reactive.Linq;
using GitHubSharp.Models;
using GitHubSharp;
using System.Collections.Generic;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Changesets
{
    public abstract class CommitsViewModel : BaseViewModel, ILoadableViewModel
	{
        private readonly ReactiveList<CommitModel> _commits = new ReactiveList<CommitModel>();

		public string RepositoryOwner { get; set; }

		public string RepositoryName { get; set; }

        public IReactiveCommand<object> GoToChangesetCommand { get; private set; }

        public IReadOnlyReactiveList<CommitViewModel> Commits { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

	    protected CommitsViewModel()
	    {
            Commits = _commits.CreateDerivedCollection(CreateViewModel, ContainsSearchKeyword, signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            GoToChangesetCommand = ReactiveCommand.Create();
            GoToChangesetCommand.OfType<CommitViewModel>().Subscribe(x =>
	        {
	            var vm = CreateViewModel<ChangesetViewModel>();
	            vm.RepositoryOwner = RepositoryOwner;
	            vm.RepositoryName = RepositoryName;
	            vm.Node = x.Sha;
                ShowViewModel(vm);
	        });

            LoadCommand = ReactiveCommand.CreateAsyncTask(x => _commits.SimpleCollectionLoad(GetRequest(), x as bool?));
	    }

        protected abstract GitHubRequest<List<CommitModel>> GetRequest();

        private bool ContainsSearchKeyword(CommitModel x)
        {
            try
            {
                return x.Commit.Message.ContainsKeyword(SearchKeyword) || GenerateCommiterName(x).ContainsKeyword(SearchKeyword);
            }
            catch
            {
                return false;
            }
        }

        private static CommitViewModel CreateViewModel(CommitModel x)
        {
            var msg = x.Commit.Message ?? string.Empty;
            var firstLine = msg.IndexOf("\n", StringComparison.Ordinal);
            var desc = firstLine > 0 ? msg.Substring(0, firstLine) : msg;

            string login = GenerateCommiterName(x);
            var date = DateTimeOffset.MinValue;
            if (x.Commit.Committer != null)
                date = x.Commit.Committer.Date;

            return new CommitViewModel
            {
                Message = desc,
                Commiter = login,
                CommitDate = date,
                Sha = x.Sha
            };
        }

        private static string GenerateCommiterName(CommitModel x)
        {
            if (x.Commit.Author != null && !string.IsNullOrEmpty(x.Commit.Author.Name))
                return x.Commit.Author.Name;
            if (x.Commit.Committer != null && !string.IsNullOrEmpty(x.Commit.Committer.Name))
                return x.Commit.Committer.Name;
            if (x.Author != null)
                return x.Author.Login;
            return x.Committer != null ? x.Committer.Login : "Unknown";
        }
            
        public class CommitViewModel
        {
            public string Message { get; set; }
            public DateTimeOffset CommitDate { get; set; }
            public string Commiter { get; set; }
            public string Sha { get; set; }
        }
	}
}

