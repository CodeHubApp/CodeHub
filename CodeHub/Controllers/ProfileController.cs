using GitHubSharp.Models;
using CodeFramework.Controllers;

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

        public override void Update(bool force)
        {
            Model = Application.Client.Users[_username].Get(force).Data;
        }
    }
}

