using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels
{
    public class StargazersViewModel : BaseViewModel, ILoadableViewModel
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

        public Task Load(bool forceDataRefresh)
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

