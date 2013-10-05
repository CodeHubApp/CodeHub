using GitHubSharp.Models;
using CodeFramework.Controllers;

namespace CodeHub.Controllers
{
    public class OrganizationsController : ListController<BasicUserModel>
	{
        public string Username { get; private set; }

        public OrganizationsController(IListView<BasicUserModel> view, string username) 
            : base(view)
		{
            Username = username;
		}

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.Users[Username].GetOrganizations(), forceDataRefresh, response => {
                RenderView(new ListModel<BasicUserModel>(response.Data, this.CreateMore(response)));
            });
        }
	}
}

