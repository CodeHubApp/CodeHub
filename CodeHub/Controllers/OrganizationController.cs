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

