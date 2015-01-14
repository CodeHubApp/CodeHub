using System;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Organizations;
using CodeHub.Core.ViewModels.Repositories;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly IApplicationService _applicationService;

        private string _username;
        public string Username 
        {
            get { return _username; }
            set
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

		public bool IsLoggedInUser
		{
			get { return string.Equals(Username, _applicationService.Account.Username); }
		}

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToFollowersCommand { get; private set; }

        public IReactiveCommand<object> GoToFollowingCommand { get; private set; }

        public IReactiveCommand<object> GoToEventsCommand { get; private set; }

        public IReactiveCommand<object> GoToOrganizationsCommand  { get; private set; }

        public IReactiveCommand<object> GoToRepositoriesCommand { get; private set; }

        public IReactiveCommand<object> GoToGistsCommand { get; private set; }

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

        public UserViewModel(IApplicationService applicationService, IActionMenuFactory actionMenuService)
        {
            _applicationService = applicationService;

            ToggleFollowingCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.IsFollowing).Select(x => x.HasValue), t => ToggleFollowing());

            GoToGistsCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<UserGistsViewModel>();
                vm.Username = Username;
                NavigateTo(vm);
            });

            GoToRepositoriesCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<UserRepositoriesViewModel>();
                vm.Username = Username;
                NavigateTo(vm);
            });

            GoToOrganizationsCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<OrganizationsViewModel>();
                vm.Username = Username;
                NavigateTo(vm);
            });

            GoToEventsCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<UserEventsViewModel>();
                vm.Username = Username;
                NavigateTo(vm);
            });

            GoToFollowingCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<UserFollowingsViewModel>();
                vm.Username = Username;
                NavigateTo(vm);
            });

            GoToFollowersCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<UserFollowersViewModel>();
                vm.Username = Username;
                NavigateTo(vm);
            });

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.IsFollowing).Select(x => x.HasValue),
                _ =>
                {
                    var menu = actionMenuService.Create(Title);
                    menu.AddButton(IsFollowing.Value ? "Unfollow" : "Follow", ToggleFollowingCommand);
                    return menu.Show();
                });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                if (!IsLoggedInUser)
                {
                    Observable.FromAsync(() => applicationService.GitHubClient.User.Followers.IsFollowingForCurrent(Username))
                        .ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => IsFollowing = x);
                }

                User = await applicationService.GitHubClient.User.Get(Username);
            });
        }
    }
}

