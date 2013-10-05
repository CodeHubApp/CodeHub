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

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.Users[_username].Repositories[_slug].GetLabels(), forceDataRefresh, response => {
                RenderView(new ListModel<LabelModel>(response.Data, this.CreateMore(response)));
            });
        }
    }
}

