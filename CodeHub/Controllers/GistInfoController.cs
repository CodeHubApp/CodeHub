using System;
using GitHubSharp.Controllers;
using GitHubSharp.Models;
using CodeFramework.Controllers;

namespace CodeHub.Controllers
{
    public class GistInfoController : Controller<GistModel>
    {
        public string Id { get; private set; }

        public GistInfoController(IView<GistModel> view, string id)
            : base(view)
        {
            Id = id;
        }

        public override void Update(bool force)
        {
            Model = Application.Client.Gists[Id].Get(force).Data;
        }
    }
}

