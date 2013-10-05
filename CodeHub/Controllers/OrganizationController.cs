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

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.Organizations[Name].Get(), forceDataRefresh, response => {
                RenderView(response.Data);
            });
        }
	}
}

