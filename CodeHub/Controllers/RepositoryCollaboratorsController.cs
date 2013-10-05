using CodeFramework.Controllers;
using GitHubSharp.Models;

namespace CodeHub.Controllers
{
    public class RepositoryCollaboratorsController : ListController<BasicUserModel>
    {
        private readonly string _username;
        private readonly string _slug;

        public RepositoryCollaboratorsController(IView<ListModel<BasicUserModel>> view, string username, string slug)
            : base(view)
        {
            _username = username;
            _slug = slug;
        }

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.Users[_username].Repositories[_slug].GetCollaborators(), forceDataRefresh, response => {
                RenderView(new ListModel<BasicUserModel>(response.Data, this.CreateMore(response)));
            });
        }
    }
}

