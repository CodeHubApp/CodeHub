using System;
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

        public override void Update(bool force)
        {
            var response = Application.Client.AuthenticatedUser.Repositories.GetStarred();
            Model = new ListModel<RepositoryModel> { Data = response.Data };
            Model.More = this.CreateMore(response);
        }
    }
}

