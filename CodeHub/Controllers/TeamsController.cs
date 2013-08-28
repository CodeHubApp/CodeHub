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
    public class TeamsController : ListController<TeamShortModel>
    {
        public TeamsController(IListView<TeamShortModel> view)
            : base(view)
        {
        }

        public override void Update(bool force)
        {
            var response = Application.Client.Teams.GetAll(force);
            Model = new ListModel<TeamShortModel> { Data = response.Data };
            Model.More = this.CreateMore(response);
        }
    }
}