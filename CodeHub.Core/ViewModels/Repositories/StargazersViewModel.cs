using System.Threading.Tasks;
using System.Windows.Input;
using CodeHub.Core.ViewModels;
using CodeHub.Core.ViewModels.User;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class StargazersViewModel : BaseUserCollectionViewModel
    {
        public string User
        {
            get;
            private set;
        }

        public string Repository
        {
            get;
            private set;
        }

        public void Init(NavObject navObject)
        {
            User = navObject.User;
            Repository = navObject.Repository;
        }

        protected override Task Load(bool forceDataRefresh)
        {
            return Users.SimpleCollectionLoad(this.GetApplication().Client.Users[User].Repositories[Repository].GetStargazers(), forceDataRefresh);
        }

        public class NavObject
        {
            public string User { get; set; }
            public string Repository { get; set; }
        }
    }

    public class WatchersViewModel : BaseUserCollectionViewModel
    {
        public string User
        {
            get;
            private set;
        }

        public string Repository
        {
            get;
            private set;
        }

        public void Init(NavObject navObject)
        {
            User = navObject.User;
            Repository = navObject.Repository;
        }

        protected override Task Load(bool forceDataRefresh)
        {
            return Users.SimpleCollectionLoad(this.GetApplication().Client.Users[User].Repositories[Repository].GetWatchers(), forceDataRefresh);
        }

        public class NavObject
        {
            public string User { get; set; }
            public string Repository { get; set; }
        }
    }
}

