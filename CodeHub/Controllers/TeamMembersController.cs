using GitHubSharp.Models;
using CodeFramework.Controllers;

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

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.Teams[Id].GetMembers(), forceDataRefresh, response => {
                RenderView(new ListModel<BasicUserModel>(response.Data, this.CreateMore(response)));
            });
        }
    }
}