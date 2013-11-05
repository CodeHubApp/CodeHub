using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels
{
    public class UserFollowersViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly CollectionViewModel<BasicUserModel> _users = new CollectionViewModel<BasicUserModel>();
        private readonly IApplicationService _application;

        public CollectionViewModel<BasicUserModel> Users
        {
            get { return _users; }
        }

        public string Name
        {
            get;
            private set;
        }

        public ICommand GoToUserCommand
        {
            get { return new MvxCommand<BasicUserModel>(x => this.ShowViewModel<ProfileViewModel>(new ProfileViewModel.NavObject { Username = x.Login })); }
        }

        public UserFollowersViewModel(IApplicationService application)
        {
            _application = application;
        }

        public void Init(NavObject navObject)
        {
            Name = navObject.Username;
        }

        public Task Load(bool forceDataRefresh)
        {
            return Users.SimpleCollectionLoad(_application.Client.Users[Name].GetFollowers(), forceDataRefresh);
        }

        public class NavObject
        {
            public string Username { get; set; }
        }
    }
}

