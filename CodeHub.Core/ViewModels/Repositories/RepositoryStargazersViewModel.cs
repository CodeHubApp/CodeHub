using System.Threading.Tasks;
using System.Windows.Input;
using CodeHub.Core.ViewModels;
using CodeHub.Core.ViewModels.User;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryStargazersViewModel : BaseUserCollectionViewModel
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

        protected override Task Load()
        {
            return Users.SimpleCollectionLoad(this.GetApplication().Client.Users[User].Repositories[Repository].GetStargazers());
        }

        public class NavObject
        {
            public string User { get; set; }
            public string Repository { get; set; }
        }
    }

    public class RepositoryWatchersViewModel : BaseUserCollectionViewModel
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

        protected override Task Load()
        {
            return Users.SimpleCollectionLoad(this.GetApplication().Client.Users[User].Repositories[Repository].GetWatchers());
        }

        public class NavObject
        {
            public string User { get; set; }
            public string Repository { get; set; }
        }
    }
}

