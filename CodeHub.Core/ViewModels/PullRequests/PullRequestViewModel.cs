using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestViewModel : CodeHub.Core.ViewModels.Issues.IssueViewModel
    {
        private PullRequestModel _model;
        private bool _merged;

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

		public ICommand GoToCommitsCommand
		{
            get { return new MvxCommand(() => ShowViewModel<PullRequestCommitsViewModel>(new PullRequestCommitsViewModel.NavObject { Username = User, Repository = Repository, PullRequestId = Id })); }
		}

		public ICommand GoToFilesCommand
		{
            get { return new MvxCommand(() => ShowViewModel<PullRequestFilesViewModel>(new PullRequestFilesViewModel.NavObject { Username = User, Repository = Repository, PullRequestId = Id })); }
		}

        protected override Task Load(bool forceDataRefresh)
        {
            var subTask = base.Load(forceDataRefresh);
            var pullRequest = this.GetApplication().Client.Users[User].Repositories[Repository].PullRequests[Id].Get();
			var t1 = this.RequestModel(pullRequest, forceDataRefresh, response => PullRequest = response.Data);
            return Task.WhenAll(subTask, t1);
        }

        public async Task Merge()
        {
            var response = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[User].Repositories[Repository].PullRequests[Id].Merge());
            if (!response.Data.Merged)
                throw new Exception(response.Data.Message);

            var pullRequest = this.GetApplication().Client.Users[User].Repositories[Repository].PullRequests[Id].Get();
            await this.RequestModel(pullRequest, true, r => PullRequest = r.Data);
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

        public class NavObject : CodeHub.Core.ViewModels.Issues.IssueViewModel.NavObject
        {
        }
    }
}
