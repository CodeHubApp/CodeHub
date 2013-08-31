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

        public override void Update(bool force)
        {
            var response = Application.Client.Organizations[Name].GetMembers(force);
            Model = new ListModel<BasicUserModel> {Data = response.Data, More = this.CreateMore(response)};
        }
    }
}

