using System.Threading.Tasks;
using System.Windows.Input;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Organizations;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels.Repositories;

namespace CodeHub.Core.ViewModels.User
{
    public class UserViewModel : LoadableViewModel
    {
        public string Username { get; private set; }

        private UserModel _user;
        public UserModel User
        {
            get { return _user; }
            private set { this.RaiseAndSetIfChanged(ref _user, value); }
        }

        private bool _isFollowing;
        public bool IsFollowing
        {
            get { return _isFollowing; }
            private set { this.RaiseAndSetIfChanged(ref _isFollowing, value); }
        }

        public bool IsLoggedInUser
        {
            get
            {
                return string.Equals(Username, this.GetApplication().Account.Username);
            }
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

        public ICommand ToggleFollowingCommand
        {
            get { return new MvxCommand(() => ToggleFollowing().ToBackground()); }
        }

        private async Task ToggleFollowing()
        {
            try
            {
                if (IsFollowing)
                    await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.AuthenticatedUser.Unfollow(Username));
                else
                    await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.AuthenticatedUser.Follow(Username));
                IsFollowing = !IsFollowing;
            }
            catch
            {
                DisplayAlert("Unable to follow user! Please try again.");
            }
        }
  
        public void Init(NavObject navObject)
        {
            Title = Username = navObject.Username;
        }

        protected override Task Load()
        {
            this.RequestModel(this.GetApplication().Client.AuthenticatedUser.IsFollowing(Username), x => IsFollowing = x.Data).FireAndForget();
            return this.RequestModel(this.GetApplication().Client.Users[Username].Get(), response => User = response.Data);
        }

        public class NavObject
        {
            public string Username { get; set; }
        }
    }
}

