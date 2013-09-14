using CodeFramework.Controllers;
using GitHubSharp.Models;

namespace CodeHub.Controllers
{
    public class RepositoryLabelsController : ListController<LabelModel>
    {
        private readonly string _username;
        private readonly string _slug;

        public RepositoryLabelsController(IView<ListModel<LabelModel>> view, string username, string slug)
            : base(view)
        {
            _username = username;
            _slug = slug;
        }

        public override void Update(bool force)
        {
            var response = Application.Client.Users[_username].Repositories[_slug].GetLabels(force);
            Model = new ListModel<LabelModel> {Data = response.Data, More = this.CreateMore(response)};
        }
    }
}

