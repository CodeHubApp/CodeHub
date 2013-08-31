using GitHubSharp.Models;
using CodeFramework.Controllers;

namespace CodeHub.Controllers
{
	public class OrganizationController : Controller<UserModel>
	{
        public string Name { get; private set; }

		public OrganizationController(IView<UserModel> view, string name) 
            : base(view)
		{
			Name = name;
		}

        public override void Update(bool force)
        {
            Model = Application.Client.Organizations[Name].Get(force).Data;
        }
	}
}

