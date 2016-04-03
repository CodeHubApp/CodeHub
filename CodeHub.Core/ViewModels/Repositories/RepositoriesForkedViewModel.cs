using System;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesForkedViewModel : RepositoriesViewModel
    {
        public RepositoriesForkedViewModel()
        {
            ShowRepositoryOwner = true;
        }

        protected override Task Load()
        {
            return Repositories.SimpleCollectionLoad(this.GetApplication().Client.Users[User].Repositories[Repository].GetForks());
        }

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

        public class NavObject
        {
            public string User { get; set; }
            public string Repository { get; set; }
        }
    }
}

