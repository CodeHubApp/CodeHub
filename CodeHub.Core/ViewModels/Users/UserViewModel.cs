using System;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Organizations;
using CodeHub.Core.ViewModels.Repositories;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;
using CodeHub.Core.Factories;
using CodeHub.Core.ViewModels.Activity;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly ISessionService _applicationService;

        private string _username;
        public string Username 
        {
            get { return _username; }
            private set
            {
                _username = value;
                Title = value;
            }
        }

        private Octokit.User _userModel;
        public Octokit.User User
        {
            get { return _userModel; }
            private set { this.RaiseAndSetIfChanged(ref _userModel, value); }
        }

        private bool? _isFollowing;
        public bool? IsFollowing
		{
			get { return _isFollowing; }
			private set { this.RaiseAndSetIfChanged(ref _isFollowing, value); }
		}

        private readonly ObservableAsPropertyHelper<bool> _hasBlog;
        public bool HasBlog
        {
            get { return _hasBlog.Value; }
        }

		public bool IsLoggedInUser
		{
			get { return string.Equals(Username, _applicationService.Account.Username); }
		}

        private readonly ObservableAsPropertyHelper<GitHubAvatar> _avatar;
        public GitHubAvatar Avatar
        {
            get { return _avatar.Value; }
        }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToFollowersCommand { get; private set; }

        public IReactiveCommand<object> GoToFollowingCommand { get; private set; }

        public IReactiveCommand<object> GoToEventsCommand { get; private set; }

        public IReactiveCommand<object> GoToOrganizationsCommand  { get; private set; }

        public IReactiveCommand<object> GoToRepositoriesCommand { get; private set; }

        public IReactiveCommand<object> GoToGistsCommand { get; private set; }

        public IReactiveCommand<object> GoToWebsiteCommand { get; private set; }

		public IReactiveCommand ToggleFollowingCommand { get; private set; }

        public IReactiveCommand<Unit> ShowMenuCommand { get; private set; }

		private async Task ToggleFollowing()
		{
		    if (!IsFollowing.HasValue) return;
            if (IsFollowing.Value)
                await _applicationService.GitHubClient.User.Followers.Unfollow(Username);
			else
                await _applicationService.GitHubClient.User.Followers.Follow(Username);
			IsFollowing = !IsFollowing.Value;
		}

        public UserViewModel(ISessionService applicationService, IActionMenuFactory actionMenuService)
        {
            _applicationService = applicationService;

            ToggleFollowingCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.IsFollowing).Select(x => x.HasValue), t => ToggleFollowing());

            _hasBlog = this.WhenAnyValue(x => x.User.Blog)
                .Select(x => !string.IsNullOrEmpty(x))
                .ToProperty(this, x => x.HasBlog);

            _avatar = this.WhenAnyValue(x => x.User.AvatarUrl)
                .Select(x => new GitHubAvatar(x))
                .ToProperty(this, x => x.Avatar);

            GoToGistsCommand = ReactiveCommand.Create();
            GoToGistsCommand
                .Select(_ => this.CreateViewModel<UserGistsViewModel>())
                .Select(x => x.Init(Username))
                .Subscribe(NavigateTo);

            GoToRepositoriesCommand = ReactiveCommand.Create();
            GoToRepositoriesCommand
                .Select(_ => this.CreateViewModel<UserRepositoriesViewModel>())
                .Select(x => x.Init(Username))
                .Subscribe(NavigateTo);

            GoToOrganizationsCommand = ReactiveCommand.Create();
            GoToOrganizationsCommand
                .Select(_ => this.CreateViewModel<OrganizationsViewModel>())
                .Select(x => x.Init(Username))
                .Subscribe(NavigateTo);
            
            GoToEventsCommand = ReactiveCommand.Create();
            GoToEventsCommand
                .Select(_ => this.CreateViewModel<UserEventsViewModel>())
                .Select(x => x.Init(Username))
                .Subscribe(NavigateTo);

            GoToFollowingCommand = ReactiveCommand.Create();
            GoToFollowingCommand
                .Select(_ => this.CreateViewModel<UserFollowingsViewModel>())
                .Select(x => x.Init(Username))
                .Subscribe(NavigateTo);

            GoToFollowersCommand = ReactiveCommand.Create();
            GoToFollowersCommand
                .Select(_ => this.CreateViewModel<UserFollowersViewModel>())
                .Select(x => x.Init(Username))
                .Subscribe(NavigateTo);

            GoToWebsiteCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.HasBlog));
            GoToWebsiteCommand
                .Select(_ => this.CreateViewModel<WebBrowserViewModel>())
                .Select(x => x.Init(User.Blog))
                .Subscribe(NavigateTo);

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.IsFollowing).Select(x => x.HasValue),
                sender => {
                    var menu = actionMenuService.Create();
                    menu.AddButton(IsFollowing.Value ? "Unfollow" : "Follow", ToggleFollowingCommand);
                    return menu.Show(sender);
                });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                if (!IsLoggedInUser)
                {
                    applicationService.GitHubClient.User.Followers.IsFollowingForCurrent(Username)
                        .ToBackground(x => IsFollowing = x);
                }

                User = await applicationService.GitHubClient.User.Get(Username);
            });
        }

        public UserViewModel Init(string username, Octokit.User user = null)
        {
            Username = username;
            User = user;
            return this;
        }
    }
}

