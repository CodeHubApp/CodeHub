using System.Collections.Generic;
using System.Linq;
using CodeHub.Data;
using GitHubSharp.Models;
using CodeHub.GitHub.Controllers.Followers;
using CodeHub;

namespace CodeHub.GitHub.Controllers.Followers
{
    public class UserFollowersController : FollowersController
    {
        private readonly string _name;
        
        public UserFollowersController(string name)
        {
            _name = name;
        }
        
        protected override object OnUpdateListModel(bool forced, int currentPage, ref int nextPage)
        {
            var f = Application.Client.API.GetUserFollowers(_name, currentPage);
            nextPage = f.Next == null ? -1 : currentPage + 1;
            return f.Data.OrderBy(x => x.Login).ToList();
        }
    }
}
