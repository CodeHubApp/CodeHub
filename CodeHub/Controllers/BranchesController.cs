using CodeFramework.Controllers;
using GitHubSharp.Models;

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

        public override void Update(bool force)
        {
            var response = Application.Client.Users[_username].Repositories[_slug].GetBranches(force);
            Model = new ListModel<BranchModel> {Data = response.Data, More = this.CreateMore(response)};
        }
    }
}

