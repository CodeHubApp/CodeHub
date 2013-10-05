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

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.Gists[Id].Get(), forceDataRefresh, response => {
                Model = response.Data;
                Refresh();
            });
        }
    }
}

