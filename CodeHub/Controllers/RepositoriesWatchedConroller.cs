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

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.AuthenticatedUser.Repositories.GetWatching(), forceDataRefresh, response => {
                RenderView(new ListModel<RepositoryModel>(response.Data, this.CreateMore(response)));
            });
        }
    }
}

