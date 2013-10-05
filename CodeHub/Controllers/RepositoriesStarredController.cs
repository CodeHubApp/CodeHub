using GitHubSharp.Models;
using CodeFramework.Controllers;

namespace CodeHub.Controllers
{
    public class RepositoriesStarredController : RepositoriesController
    {
        public RepositoriesStarredController(IListView<RepositoryModel> view)
            : base(view, string.Empty)
        {
        }

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.AuthenticatedUser.Repositories.GetStarred(), forceDataRefresh, response => {
                RenderView(new ListModel<RepositoryModel>(response.Data, this.CreateMore(response)));
            });
        }
    }
}

