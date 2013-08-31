using GitHubSharp.Models;
using CodeFramework.Controllers;

namespace CodeHub.Controllers
{
    public class TeamMembersController : ListController<BasicUserModel>
    {
        public long Id { get; private set; }

		public TeamMembersController(IListView<BasicUserModel> view, long id)
            : base(view)
        {
            Id = id;
        }

        public override void Update(bool force)
        {
            var response = Application.Client.Teams[Id].GetMembers(force);
            Model = new ListModel<BasicUserModel> {Data = response.Data, More = this.CreateMore(response)};
        }
    }
}