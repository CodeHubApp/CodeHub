using CodeFramework.Controllers;
using GitHubSharp.Models;
using System.Collections.Generic;

namespace CodeHub.Controllers
{
    public class BranchesController : ListController<BranchModel>
    {
        private readonly string _username;
        private readonly string _slug;

        public BranchesController(IView<ListModel<BranchModel>> view, string username, string slug)
            : base(view)
        {
            _username = username;
            _slug = slug;
        }

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.Users[_username].Repositories[_slug].GetBranches(), forceDataRefresh, response => {
                RenderView(new ListModel<BranchModel>(response.Data, this.CreateMore(response)));
            });
        }
    }
}

