using GitHubSharp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeFramework.Controllers;

namespace CodeHub.Controllers
{
    public class UserFollowersController : ListController<BasicUserModel>
    {
        private readonly string _name;
        
        public UserFollowersController(IView<ListModel<BasicUserModel>> view, string name)
            : base(view)
        {
            _name = name;
        }

        public override void Update(bool force)
        {
            var response = Application.Client.Users[_name].GetFollowers();
            Model = new ListModel<BasicUserModel> { Data = response.Data };
            Model.More = this.CreateMore(response);
        }
    }
}

