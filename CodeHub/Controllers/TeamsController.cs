using GitHubSharp.Models;
using CodeFramework.Controllers;

namespace CodeHub.Controllers
{
    public class TeamsController : ListController<TeamShortModel>
    {
        public string OrganizationName { get; private set; }

        public TeamsController(IListView<TeamShortModel> view, string organizationName)
            : base(view)
        {
            OrganizationName = organizationName;
        }

        public override void Update(bool force)
        {
            var response = Application.Client.Organizations[OrganizationName].GetTeams(force);
            Model = new ListModel<TeamShortModel> {Data = response.Data, More = this.CreateMore(response)};
        }
    }
}