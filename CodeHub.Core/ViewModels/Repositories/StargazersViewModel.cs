using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.ViewModels.User;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class StargazersViewModel : LoadableViewModel
    {
        private readonly CollectionViewModel<BasicUserModel> _stargazers = new CollectionViewModel<BasicUserModel>();

        public CollectionViewModel<BasicUserModel> Stargazers
        {
            get
            {
                return _stargazers;
            }
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

        public ICommand GoToUserCommand
        {
            get { return new MvxCommand<BasicUserModel>(x => this.ShowViewModel<ProfileViewModel>(new ProfileViewModel.NavObject { Username = x.Login })); }
        }

        public void Init(NavObject navObject)
        {
            User = navObject.User;
            Repository = navObject.Repository;
        }

        protected override Task Load(bool forceDataRefresh)
        {
            return Stargazers.SimpleCollectionLoad(Application.Client.Users[User].Repositories[Repository].GetStargazers(), forceDataRefresh);
        }

        public class NavObject
        {
            public string User { get; set; }
            public string Repository { get; set; }
        }
    }
}

