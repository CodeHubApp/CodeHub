using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestViewModel : LoadableViewModel
    {
        private PullRequestModel _model;
        private bool _merged;
        private readonly CollectionViewModel<IssueCommentModel> _comments = new CollectionViewModel<IssueCommentModel>();

        public string User 
        { 
            get; 
            private set; 
        }

        public string Repo 
        { 
            get; 
            private set; 
        }

        public ulong PullRequestId 
        { 
            get; 
            private set; 
        }

        public bool Merged
        {
            get { return _merged; }
            set { _merged = value; RaisePropertyChanged(() => Merged); }
        }

        public PullRequestModel PullRequest 
        { 
            get { return _model; }
            set
            {
                _model = value;
                RaisePropertyChanged(() => PullRequest);
            }
        }

        public CollectionViewModel<IssueCommentModel> Comments
        {
            get { return _comments; }
        }

		public ICommand GoToCommitsCommand
		{
			get { return new MvxCommand(() => ShowViewModel<PullRequestCommitsViewModel>(new PullRequestCommitsViewModel.NavObject { Username = User, Repository = Repo, PullRequestId = PullRequestId })); }
		}

		public ICommand GoToFilesCommand
		{
			get { return new MvxCommand(() => ShowViewModel<PullRequestFilesViewModel>(new PullRequestFilesViewModel.NavObject { Username = User, Repository = Repo, PullRequestId = PullRequestId })); }
		}

        public void Init(NavObject navObject)
        {
            User = navObject.Username;
            Repo = navObject.Repository;
            PullRequestId = navObject.Id;
        }

        protected override Task Load(bool forceDataRefresh)
        {
			var pullRequest = this.GetApplication().Client.Users[User].Repositories[Repo].PullRequests[PullRequestId].Get();
			var commentsRequest = this.GetApplication().Client.Users[User].Repositories[Repo].Issues[PullRequestId].GetComments();

            var t1 = Task.Run(() => this.RequestModel(pullRequest, forceDataRefresh, response => PullRequest = response.Data));

            FireAndForgetTask.Start(() => this.RequestModel(commentsRequest, forceDataRefresh, response => {
                Comments.Items.Reset(response.Data);
                this.CreateMore(response, m => Comments.MoreItems = m, d => Comments.Items.AddRange(d));
            }));

            return t1;
        }

        public async Task AddComment(string text)
        {
			var comment = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[User].Repositories[Repo].Issues[PullRequestId].CreateComment(text));
            Comments.Items.Add(comment.Data);
        }

        public async Task Merge()
        {
			var response = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[User].Repositories[Repo].PullRequests[PullRequestId].Merge());
            if (!response.Data.Merged)
                throw new Exception(response.Data.Message);

			var pullRequest = this.GetApplication().Client.Users[User].Repositories[Repo].PullRequests[PullRequestId].Get();
            await Task.Run(() => this.RequestModel(pullRequest, true, r => PullRequest = r.Data));
        }

        public ICommand MergeCommand
        {
            get { return new MvxCommand(() => Merge(), CanMerge); }
        }

        private bool CanMerge()
        {
            if (PullRequest == null)
                return false;
            return (PullRequest.Merged != null && PullRequest.Merged.Value == false && (PullRequest.Mergable == null || PullRequest.Mergable.Value));
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public ulong Id { get; set; }
        }
    }
}
