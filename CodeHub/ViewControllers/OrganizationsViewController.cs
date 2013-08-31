using CodeFramework.Controllers;
using System.Collections.Generic;
using GitHubSharp.Models;
using MonoTouch.UIKit;
using CodeHub.Controllers;
using MonoTouch.Dialog;

namespace CodeHub.ViewControllers
{
    public class OrganizationsViewController : BaseListControllerDrivenViewController, IListView<BasicUserModel>
	{
        private readonly string _username;

		public OrganizationsViewController(string username) 
		{
            _username = username;
            Title = "Groups".t();
            SearchPlaceholder = "Search Groups".t();
            NoItemsText = "No Groups".t();
            Controller = new OrganizationsController(this, username);
		}

        public void Render(ListModel<BasicUserModel> model)
        {
            RenderList(model, x => {
                return new StyledStringElement(x.Login, () => NavigationController.PushViewController(new OrganizationViewController(x.Login), true));
            });
        }
	}
}

