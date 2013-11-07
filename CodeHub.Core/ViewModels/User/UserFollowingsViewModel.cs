using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Services;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.User
{
    public class UserFollowingsViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly CollectionViewModel<BasicUserModel> _users = new CollectionViewModel<BasicUserModel>();
        private readonly IApplicationService _application;

        public string Name
        {
            get;
            private set;
        }

        public CollectionViewModel<BasicUserModel> Users
        {
            get { return _users; }
        }

        public ICommand GoToUserCommand
        {
            get { return new MvxCommand<BasicUserModel>(x => this.ShowViewModel<ProfileViewModel>(new ProfileViewModel.NavObject { Username = x.Login }));}
        }

        public UserFollowingsViewModel(IApplicationService application)
        {
            _application = application;
        }

        public void Init(NavObject navObject)
        {
            Name = navObject.Name;
        }

        public Task Load(bool forceDataRefresh)
        {
            return _users.SimpleCollectionLoad(_application.Client.Users[Name].GetFollowing(), forceDataRefresh);
        }

        public class NavObject
        {
            public string Name { get; set; }
        }
    }
}