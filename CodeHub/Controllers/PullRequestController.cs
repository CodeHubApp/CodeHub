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
            
        }

        public class ViewModel
        {
            public PullRequestModel PullRequest { get; set; }
            public List<CommentModel> Comments { get; set; }
        }
    }
}
