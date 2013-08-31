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
    public class TeamsViewController : BaseListControllerDrivenViewController, IListView<TeamShortModel>
    {
        public TeamsViewController(string organizationName) 
        {
            Title = "Teams".t();
            SearchPlaceholder = "Search Teams".t();
            NoItemsText = "No Teams".t();
            Controller = new TeamsController(this, organizationName);
        }

        public void Render(ListModel<TeamShortModel> model)
        {
            RenderList(model, o => new StyledStringElement(o.Name, () => NavigationController.PushViewController(new TeamMembersViewController(o.Name, o.Id), true)));
        }
    }
}