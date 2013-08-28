using MonoTouch.Dialog;
using GitHubSharp.Models;
using System.Collections.Generic;
using MonoTouch.UIKit;
using System.Linq;
using CodeHub.Controllers;
using CodeFramework.Controllers;
using CodeFramework.Elements;
using System.Threading.Tasks;

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
            Model = new ListModel<BasicUserModel> { Data = response.Data };
            Model.More = this.CreateMore(response);
        }
	}
}

