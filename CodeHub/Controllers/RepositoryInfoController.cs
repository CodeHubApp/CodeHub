using System;
using CodeFramework.Controllers;
using GitHubSharp.Models;

namespace CodeHub.Controllers
{
    public class RepositoryInfoController : Controller<RepositoryModel>
    {
        public string User { get; private set; }

        public string Repo { get; private set; }

        public RepositoryInfoController(IView<RepositoryModel> view, string user, string repo)
            : base(view)
        {
            User = user;
            Repo = repo;
        }

        public override void Update(bool force)
        {
            Model = Application.Client.Users[User].Repositories[Repo].Get(force).Data;
        }
    }
}

