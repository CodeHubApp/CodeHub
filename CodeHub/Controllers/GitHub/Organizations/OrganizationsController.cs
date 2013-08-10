using CodeHub.Controllers;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using System.Collections.Generic;
using System.Linq;
using CodeFramework.Controllers;
using CodeFramework.Elements;

namespace CodeHub.GitHub.Controllers.Organizations
{
	public class OrganizationsController : BaseListModelController
	{
        public string Username { get; private set; }

        public OrganizationsController(string username, bool push = true) 
            : base(typeof(List<BasicUserModel>))
		{
            Username = username;
            Title = "Organizations";
		}

        protected override Element CreateElement(object obj)
        {
            var userModel = (BasicUserModel)obj;
            return new StyledStringElement(userModel.Login, () => NavigationController.PushViewController(new OrganizationInfoController(userModel.Login), true));
        }

        protected override object OnUpdateListModel(bool forced, int currentPage, ref int nextPage)
        {
            var f = Application.Client.API.GetOrganizations(Username);
            return f.Data.OrderBy(x => x.Login).ToList();
        }
	}
}

