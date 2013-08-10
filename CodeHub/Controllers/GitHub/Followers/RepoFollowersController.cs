using System.Linq;
using GitHubSharp.Models;
using System.Collections.Generic;
using CodeHub.Data;
using CodeHub.GitHub.Controllers.Followers;
using CodeHub;

namespace CodeHub.GitHub.Controllers.Followers
{
    public class RepoFollowersController : FollowersController
    {
        private readonly string _name;
        private readonly string _owner;
        
        public RepoFollowersController(string owner, string name)
        {
            _name = name;
            _owner = owner;
        }

        protected override object OnUpdateListModel(bool forced, int currentPage, ref int nextPage)
        {
            var f = Application.Client.API.GetRepositoryWatchers(_owner, _name, currentPage);
            nextPage = f.Next == null ? -1 : currentPage + 1;
            return f.Data.OrderBy(x => x.Login).ToList();
        }
    }
}
