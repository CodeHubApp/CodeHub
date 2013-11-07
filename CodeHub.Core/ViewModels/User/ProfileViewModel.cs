using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Gists;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.User
{
    public class ProfileViewModel : BaseViewModel, ILoadableViewModel
    {
        private UserModel _userModel;
        private readonly IApplicationService _application;

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
            get { return new MvxCommand(() => ShowViewModel<UserFollowingsViewModel>(new UserFollowingsViewModel.NavObject { Name = Username })); }
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

        public ProfileViewModel(IApplicationService application)
        {
            _application = application;
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
        }

        public Task Load(bool forceDataRefresh)
        {
            return Task.Run(() => this.RequestModel(_application.Client.Users[Username].Get(), forceDataRefresh, response => User = response.Data));
        }

        public class NavObject
        {
            public string Username { get; set; }
        }
    }
}

