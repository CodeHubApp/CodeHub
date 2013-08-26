using System;
using GitHubSharp.Models;
using System.Threading;
using MonoTouch.Foundation;
using CodeFramework.Controllers;

namespace CodeHub.Controllers
{
    public class ProfileController : Controller<UserModel>
    {
        private string _username;

        public ProfileController(IView<UserModel> view, string username)
            : base(view)
        {
            _username = username;
        }

        public override void Update(bool forced)
        {
            Model = Application.Client.Users[_username].GetInfo().Data;
        }
    }
}

