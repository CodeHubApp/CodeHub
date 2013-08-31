using GitHubSharp.Models;
using CodeFramework.Controllers;

namespace CodeHub.Controllers
{
    public class RepositoriesWatchedController : RepositoriesController
    {
        public RepositoriesWatchedController(IListView<RepositoryModel> view)
            : base(view, string.Empty)
        {
        }

        public override void Update(bool force)
        {
            var response = Application.Client.AuthenticatedUser.Repositories.GetWatching(force);
            Model = new ListModel<RepositoryModel> {Data = response.Data, More = this.CreateMore(response)};
        }
    }
}

