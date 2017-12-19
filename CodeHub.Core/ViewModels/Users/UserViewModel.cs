using System.Threading.Tasks;
using System.Windows.Input;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Organizations;
using MvvmCross.Core.ViewModels;
using Splat;

namespace CodeHub.Core.ViewModels.User
{
    public class UserViewModel : LoadableViewModel
    {
        private readonly IApplicationService _applicationService = Locator.Current.GetService<IApplicationService>();

        public string Username { get; private set; }

        private Octokit.User _user;
        public Octokit.User User
        {
            get { return _user; }
            set { this.RaiseAndSetIfChanged(ref _user, value); }
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

        public ICommand GoToEventsCommand
        {
            get { return new MvxCommand(() => ShowViewModel<UserEventsViewModel>(new UserEventsViewModel.NavObject { Username = Username })); }
        }

        public ICommand GoToOrganizationsCommand
        {
            get { return new MvxCommand(() => ShowViewModel<OrganizationsViewModel>(new OrganizationsViewModel.NavObject { Username = Username })); }
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
                    await _applicationService.GitHubClient.User.Followers.Unfollow(Username);
                else
                    await _applicationService.GitHubClient.User.Followers.Follow(Username);
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

        protected override async Task Load()
        {
            _applicationService.GitHubClient.User.Followers
                .IsFollowingForCurrent(Username).ToBackground(x => IsFollowing = x);

            if (User != null)
                _applicationService.GitHubClient.User.Get(Username).ToBackground(x => User = x);
            else
                User = await _applicationService.GitHubClient.User.Get(Username);
        }

        public class NavObject
        {
            public string Username { get; set; }
        }
    }
}

