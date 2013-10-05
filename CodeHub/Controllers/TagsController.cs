using CodeFramework.Controllers;
using GitHubSharp.Models;

namespace CodeHub.Controllers
{
    public class TagsController : ListController<TagModel>
    {
        private readonly string _username;
        private readonly string _slug;

        public TagsController(IListView<TagModel> view, string username, string slug)
            : base(view)
        {
            _username = username;
            _slug = slug;
        }

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.Users[_username].Repositories[_slug].GetTags(), forceDataRefresh, response => {
                RenderView(new ListModel<TagModel>(response.Data, this.CreateMore(response)));
            });
        }
    }
}

