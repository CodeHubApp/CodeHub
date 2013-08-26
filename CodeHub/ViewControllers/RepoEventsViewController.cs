using GitHubSharp.Models;
using CodeHub.Controllers;

namespace CodeHub.ViewControllers
{
    public class RepoEventsViewController : EventsViewController
    {
        public RepoEventsViewController(string username, string slug)
            : base(username)
        {
            ReportRepository = false;
            Controller = new RepositoryEventsController(this, username, slug);
        }
    }
}

