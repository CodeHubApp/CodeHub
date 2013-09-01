using GitHubSharp.Models;
using CodeHub.Controllers;

namespace CodeHub.ViewControllers
{
    public class RepoEventsViewController : BaseEventsViewController
    {
        public RepoEventsViewController(string username, string slug)
        {
            ReportRepository = false;
            Controller = new RepositoryEventsController(this, username, slug);
        }
    }
}

