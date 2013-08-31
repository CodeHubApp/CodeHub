using MonoTouch.Dialog;
using GitHubSharp.Models;
using System.Collections.Generic;
using MonoTouch.UIKit;
using System.Linq;
using CodeHub.Controllers;
using CodeFramework.Controllers;
using CodeFramework.Elements;
using System.Threading.Tasks;

namespace CodeHub.ViewControllers
{
    public class TeamMembersViewController : BaseListControllerDrivenViewController, IListView<BasicUserModel>
    {
		public TeamMembersViewController(string name, long id) 
        {
			Title = name;
            SearchPlaceholder = "Search Members".t();
            NoItemsText = "No Members".t();
            Controller = new TeamMembersController(this, id);
        }

		public void Render(ListModel<BasicUserModel> model)
        {
            RenderList(model, s => {
                StyledStringElement sse = new UserElement(s.Login, string.Empty, string.Empty, s.AvatarUrl);
                sse.Tapped += () => NavigationController.PushViewController(new ProfileViewController(s.Login), true);
                return sse;
            });
        }
    }
}