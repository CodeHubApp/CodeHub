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
    public class TeamMembersController : ListController<BasicUserModel>
    {
        public long Id { get; private set; }

		public TeamMembersController(IListView<BasicUserModel> view, long id)
            : base(view)
        {
            Id = id;
        }

        public override void Update(bool force)
        {
            var response = Application.Client.Teams[Id].GetMembers(force);
			Model = new ListModel<BasicUserModel> { Data = response.Data };
            Model.More = this.CreateMore(response);
        }
    }
}