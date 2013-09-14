using CodeFramework.Controllers;
using GitHubSharp.Models;

namespace CodeHub.Controllers
{
    public class IssueMilestonesController : ListController<MilestoneModel>
    {
        private readonly string _username;
        private readonly string _slug;

        public IssueMilestonesController(IView<ListModel<MilestoneModel>> view, string username, string slug)
            : base(view)
        {
            _username = username;
            _slug = slug;
        }

        public override void Update(bool force)
        {
            var response = Application.Client.Users[_username].Repositories[_slug].Milestones.GetAll(force);
            Model = new ListModel<MilestoneModel> {Data = response.Data, More = this.CreateMore(response)};
        }
    }
}

