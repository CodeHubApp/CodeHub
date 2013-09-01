using GitHubSharp.Models;
using CodeFramework.Controllers;

namespace CodeHub.Controllers
{
    public class UserFollowingsController : ListController<BasicUserModel>
    {
        private readonly string _name;

        public UserFollowingsController(IView<ListModel<BasicUserModel>> view, string name)
            : base(view)
        {
            _name = name;
        }

        public override void Update(bool force)
        {
            var response = Application.Client.Users[_name].GetFollowing(force);
            Model = new ListModel<BasicUserModel> {Data = response.Data, More = this.CreateMore(response)};
        }
    }
}

