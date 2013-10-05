using System.Collections.Generic;
using CodeFramework.Controllers;
using GitHubSharp.Models;

namespace CodeHub.Controllers
{
    public class PullRequestController : Controller<PullRequestController.ViewModel>
    {
        public string User { get; private set; }

        public string Repo { get; private set; }

        public long PullRequestId { get; private set; }

        public PullRequestController(IView<ViewModel> view, string user, string repo, long pullRequestId)
            : base(view)
        {
            User = user;
            Repo = repo;
            PullRequestId = pullRequestId;
        }

        protected override void OnUpdate(bool forceDataRefresh)
        {
            var pull = Application.Client.Users[User].Repositories[Repo].PullRequests[PullRequestId];
            var comments = Application.Client.Execute(Application.Client.Users[User].Repositories[Repo].Issues[PullRequestId].GetComments());
            Model = new ViewModel {
                PullRequest = Application.Client.Execute(pull.Get()).Data,
                Comments = comments.Data
            };
        }

        public void AddComment(string text)
        {
            var comment = Application.Client.Execute(Application.Client.Users[User].Repositories[Repo].Issues[PullRequestId].CreateComment(text));
            Model.Comments.Add(comment.Data);
            RenderView();
        }

        public class ViewModel
        {
            public PullRequestModel PullRequest { get; set; }
            public List<IssueCommentModel> Comments { get; set; }
        }
    }
}
