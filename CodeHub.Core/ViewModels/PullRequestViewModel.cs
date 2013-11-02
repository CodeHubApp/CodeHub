using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;
using System.Threading.Tasks;
using System;

namespace CodeHub.Core.ViewModels
{
    public class PullRequestViewModel : BaseViewModel, ILoadableViewModel
    {
        private PullRequestModel _model;
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

        public void Init(NavObject navObject)
        {
            User = navObject.Username;
            Repo = navObject.Repository;
            PullRequestId = navObject.PullRequestId;
        }

        public Task Load(bool forceDataRefresh)
        {
            var pullRequest = Application.Client.Users[User].Repositories[Repo].PullRequests[PullRequestId].Get();
            var commentsRequest = Application.Client.Users[User].Repositories[Repo].Issues[PullRequestId].GetComments();

            var t1 = Task.Run(() => this.RequestModel(pullRequest, forceDataRefresh, response => PullRequest = response.Data));

            FireAndForgetTask.Start(() => this.RequestModel(commentsRequest, forceDataRefresh, response => {
                Comments.Items.Reset(response.Data);
                this.CreateMore(response, m => Comments.MoreItems = m, d => Comments.Items.AddRange(d));
            }));

            return t1;
        }

        public async Task AddComment(string text)
        {
            var comment = await Application.Client.ExecuteAsync(Application.Client.Users[User].Repositories[Repo].Issues[PullRequestId].CreateComment(text));
            Comments.Items.Add(comment.Data);
        }

        public async Task Merge()
        {
            var response = await Application.Client.ExecuteAsync(Application.Client.Users[User].Repositories[Repo].PullRequests[PullRequestId].Merge());
            if (!response.Data.Merged)
                throw new Exception(response.Data.Message);

            var pullRequest = Application.Client.Users[User].Repositories[Repo].PullRequests[PullRequestId].Get();
            await Task.Run(() => this.RequestModel(pullRequest, true, r => PullRequest = r.Data));
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public ulong PullRequestId { get; set; }
        }
    }
}
