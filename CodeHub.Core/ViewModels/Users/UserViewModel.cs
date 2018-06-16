using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using ReactiveUI;
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

        public ReactiveCommand<Unit, Unit> ToggleFollowingCommand { get; }

        public UserViewModel(string username)
        {
            Username = username;

            Title = username;

            ToggleFollowingCommand = ReactiveCommand.CreateFromTask(ToggleFollowing);

            ToggleFollowingCommand
                .ThrownExceptions
                .Select(err => new UserError("Error attempting to follow user! Please try again.", err))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();
        }

        private async Task ToggleFollowing()
        {
            if (IsFollowing)
                await _applicationService.GitHubClient.User.Followers.Unfollow(Username);
            else
                await _applicationService.GitHubClient.User.Followers.Follow(Username);
            IsFollowing = !IsFollowing;
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
    }
}

