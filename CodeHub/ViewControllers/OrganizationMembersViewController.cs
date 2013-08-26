using GitHubSharp.Models;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Linq;
using CodeHub.Controllers;
using CodeFramework.Controllers;
using CodeFramework.Elements;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CodeHub.ViewControllers
{
    public class OrganizationMembersViewController : BaseListControllerDrivenViewController, IListView<BasicUserModel>
    {
        public OrganizationMembersViewController(string user)
        {
            SearchPlaceholder = "Search Memebers".t();
            NoItemsText = "No Members".t();
            Title = user;
            Controller = new OrganizationMembersController(this, user);
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

