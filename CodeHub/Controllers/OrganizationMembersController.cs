using CodeFramework.Controllers;
using GitHubSharp.Models;

namespace CodeHub.Controllers
{
    public class OrganizationMembersController : ListController<BasicUserModel>
    {
        public string Name { get; private set; }
        
        public OrganizationMembersController(IListView<BasicUserModel> view, string name)
            : base(view)
        {
            Name = name;
        }

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.Organizations[Name].GetMembers(), forceDataRefresh, response => {
                RenderView(new ListModel<BasicUserModel>(response.Data, this.CreateMore(response)));
            });
        }
    }
}

