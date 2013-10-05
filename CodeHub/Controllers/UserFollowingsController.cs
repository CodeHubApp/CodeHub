
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

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.Users[_name].GetFollowing(), forceDataRefresh, response => {
                RenderView(new ListModel<BasicUserModel>(response.Data, this.CreateMore(response)));
            });
        }
    }
}

