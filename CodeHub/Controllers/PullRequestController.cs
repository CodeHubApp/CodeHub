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

        public override void Update(bool force)
        {
            var pull = Application.Client.Users[User].Repositories[Repo].PullRequests[PullRequestId];
            var comments = Application.Client.Users[User].Repositories[Repo].Issues[PullRequestId].GetComments(force);
            Model = new ViewModel {
                PullRequest = pull.Get(force).Data,
                Comments = comments.Data
            };
        }

        public void AddComment(string text)
        {
            var comment = Application.Client.Users[User].Repositories[Repo].Issues[PullRequestId].CreateComment(text);
            Model.Comments.Add(comment.Data);
            Render();
        }

        public class ViewModel
        {
            public PullRequestModel PullRequest { get; set; }
            public List<IssueCommentModel> Comments { get; set; }
        }
    }
}
