using CodeFramework.Controllers;
using GitHubSharp.Models;
using System.Collections.Generic;

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
            var response = Application.Client.Organizations[Name].GetMembers();
            Model = new ListModel<BasicUserModel> { Data = response.Data };
            Model.More = this.CreateMore(response);
        }
    }
}

