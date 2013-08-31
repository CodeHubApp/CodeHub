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

        public override void Update(bool force)
        {
            var response = Application.Client.Users[Username].GetOrganizations(force);
            Model = new ListModel<BasicUserModel> {Data = response.Data, More = this.CreateMore(response)};
        }
	}
}

