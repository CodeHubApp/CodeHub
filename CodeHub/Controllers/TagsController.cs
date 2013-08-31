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

        public override void Update(bool force)
        {
            var response = Application.Client.Users[_username].Repositories[_slug].GetTags(force);
            Model = new ListModel<TagModel> {Data = response.Data, More = this.CreateMore(response)};
        }
    }
}

