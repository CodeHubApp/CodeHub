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

        public override void Update(bool force)
        {
            var response = Application.Client.Users[_username].Repositories[_slug].GetCollaborators(force);
            Model = new ListModel<BasicUserModel> {Data = response.Data, More = this.CreateMore(response)};
        }
    }
}

