using GitHubSharp.Models;
using CodeFramework.Controllers;
using GitHubSharp;

namespace CodeHub.Controllers
{
    public class ProfileController : Controller<UserModel>
    {
        private readonly string _username;

        public ProfileController(IView<UserModel> view, string username)
            : base(view)
        {
            _username = username;
        }

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.Users[_username].Get(), forceDataRefresh, response => {
                RenderView(response.Data);
            });
        }
    }
}

