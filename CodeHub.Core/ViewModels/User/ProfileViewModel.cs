using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Organizations;
using GitHubSharp.Models;
using CodeFramework.Core.ViewModels;

namespace CodeHub.Core.ViewModels.User
{
    public class ProfileViewModel : LoadableViewModel
    {
        private UserModel _userModel;

        public string Username
        {
            get;
            private set;
        }

        public UserModel User
        {
            get { return _userModel; }
            private set { _userModel = value; RaisePropertyChanged(() => User); }
        }

        public ICommand GoToFollowersCommand
        {
            get { return new MvxCommand(() => ShowViewModel<UserFollowersViewModel>(new UserFollowersViewModel.NavObject { Username = Username })); }
        }

        public ICommand GoToFollowingCommand
        {
            get { return new MvxCommand(() => ShowViewModel<UserFollowingsViewModel>(new UserFollowingsViewModel.NavObject { Name = Username })); }
        }

        public ICommand GoToEventsCommand
        {
            get { return new MvxCommand(() => ShowViewModel<UserEventsViewModel>(new UserEventsViewModel.NavObject { Username = Username })); }
        }

        public ICommand GoToOrganizationsCommand
        {
            get { return new MvxCommand(() => ShowViewModel<OrganizationsViewModel>(new OrganizationsViewModel.NavObject { Username = Username })); }
        }

        public ICommand GoToRepositoriesCommand
        {
            get { return new MvxCommand(() => ShowViewModel<UserRepositoriesViewModel>(new UserRepositoriesViewModel.NavObject { Username = Username })); }
        }

        public ICommand GoToGistsCommand
        {
            get { return new MvxCommand(() => ShowViewModel<UserGistsViewModel>(new UserGistsViewModel.NavObject { Username = Username })); }
        }
  
        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
        }

        protected override Task Load(bool forceDataRefresh)
        {
			return Task.Run(() => this.RequestModel(this.GetApplication().Client.Users[Username].Get(), forceDataRefresh, response => User = response.Data));
        }

        public class NavObject
        {
            public string Username { get; set; }
        }
    }
}

