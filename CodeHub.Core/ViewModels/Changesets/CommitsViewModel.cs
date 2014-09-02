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
        private const string GitHubDefaultGravitar = "https%3A%2F%2Fassets-cdn.github.com%2Fimages%2Fgravatars%2Fgravatar-user-420.png&r=x&s=140";
        private MD5 _md5 = MD5.Create();

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
                CreateViewModel, 
                ContainsSearchKeyword, 
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            GoToChangesetCommand = ReactiveCommand.Create();
            GoToChangesetCommand.OfType<CommitItemViewModel>().Subscribe(x =>
	        {
	            var vm = CreateViewModel<ChangesetViewModel>();
	            vm.RepositoryOwner = RepositoryOwner;
	            vm.RepositoryName = RepositoryName;
	            //vm.Node = x.Sha;
                ShowViewModel(vm);
	        });

            LoadCommand = ReactiveCommand.CreateAsyncTask(x => commits.SimpleCollectionLoad(GetRequest(), x as bool?));
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

        private CommitItemViewModel CreateViewModel(CommitModel x)
        {
            var msg = x.Commit.Message ?? string.Empty;
            var firstLine = msg.IndexOf("\n", StringComparison.Ordinal);
            var desc = firstLine > 0 ? msg.Substring(0, firstLine) : msg;

            string login = GenerateCommiterName(x);
            var date = DateTimeOffset.MinValue;
            if (x.Commit.Committer != null)
                date = x.Commit.Committer.Date;

            var avatar = default(string);
            try
            {
                avatar = CreateGravatarUrl(x.Commit.Author.Email);
            }
            catch {}

            return new CommitItemViewModel(login, avatar, desc, date, _ => {});
        }

        public string CreateGravatarUrl(string email)
        {
            var inputBytes = Encoding.ASCII.GetBytes(email.Trim().ToLower());
            var hash = _md5.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));
            return string.Format("http://www.gravatar.com/avatar/{0}?d={1}", sb, GitHubDefaultGravitar);
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
	}
}

