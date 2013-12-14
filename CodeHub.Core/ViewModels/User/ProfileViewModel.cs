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
		private bool _isFollowing;

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

		public bool IsFollowing
		{
			get { return _isFollowing; }
			private set
			{
				_isFollowing = value;
				RaisePropertyChanged(() => IsFollowing);
			}
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
			get { return new MvxCommand(ToggleFollowing); }
		}

		private async void ToggleFollowing()
		{
			try
			{
				if (IsFollowing)
					await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.AuthenticatedUser.Unfollow(Username));
				else
					await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.AuthenticatedUser.Follow(Username));
				IsFollowing = !IsFollowing;
			}
			catch (System.Exception e)
			{
				ReportError(e);
			}
		}
  
        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
        }

        protected override Task Load(bool forceCacheInvalidation)
        {
			this.RequestModel(this.GetApplication().Client.AuthenticatedUser.IsFollowing(Username), forceCacheInvalidation, x => IsFollowing = x.Data).FireAndForget();
			return this.RequestModel(this.GetApplication().Client.Users[Username].Get(), forceCacheInvalidation, response => User = response.Data);
        }

        public class NavObject
        {
            public string Username { get; set; }
        }
    }
}

