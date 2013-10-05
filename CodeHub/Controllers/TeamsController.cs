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

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.Organizations[OrganizationName].GetTeams(), forceDataRefresh, response => {
                RenderView(new ListModel<TeamShortModel>(response.Data, this.CreateMore(response)));
            });
        }
    }
}