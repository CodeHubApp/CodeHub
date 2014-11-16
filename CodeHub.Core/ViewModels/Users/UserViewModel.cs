using System;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Organizations;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
using System.Reactive.Linq;
using System.Reactive;

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

        private UserModel _userModel;
        public UserModel User
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

        public IReactiveCommand LoadCommand { get; private set; }

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
				await _applicationService.Client.ExecuteAsync(_applicationService.Client.AuthenticatedUser.Unfollow(Username));
			else
				await _applicationService.Client.ExecuteAsync(_applicationService.Client.AuthenticatedUser.Follow(Username));
			IsFollowing = !IsFollowing.Value;
		}

        public UserViewModel(IApplicationService applicationService, IActionMenuService actionMenuService)
        {
            _applicationService = applicationService;

            ToggleFollowingCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.IsFollowing).Select(x => x.HasValue), t => ToggleFollowing());

            GoToGistsCommand = ReactiveCommand.Create();
            GoToGistsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserGistsViewModel>();
                vm.Username = Username;
                ShowViewModel(vm);
            });

            GoToRepositoriesCommand = ReactiveCommand.Create();
            GoToRepositoriesCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserRepositoriesViewModel>();
                vm.Username = Username;
                ShowViewModel(vm);
            });

            GoToOrganizationsCommand = ReactiveCommand.Create();
            GoToOrganizationsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<OrganizationsViewModel>();
                vm.Username = Username;
                ShowViewModel(vm);
            });

            GoToEventsCommand = ReactiveCommand.Create();
            GoToEventsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserEventsViewModel>();
                vm.Username = Username;
                ShowViewModel(vm);
            });

            GoToFollowingCommand = ReactiveCommand.Create();
            GoToFollowingCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserFollowingsViewModel>();
                vm.Username = Username;
                ShowViewModel(vm);
            });

            GoToFollowersCommand = ReactiveCommand.Create();
            GoToFollowersCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserFollowersViewModel>();
                vm.Username = Username;
                ShowViewModel(vm);
            });

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.IsFollowing).Select(x => x.HasValue),
                _ =>
                {
                var menu = actionMenuService.Create(Title);
                    menu.AddButton(IsFollowing.Value ? "Unfollow" : "Follow", ToggleFollowingCommand);
                    return menu.Show();
                });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
            {
                this.RequestModel(applicationService.Client.AuthenticatedUser.IsFollowing(Username), t as bool?, x => IsFollowing = x.Data).FireAndForget();
                return this.RequestModel(applicationService.Client.Users[Username].Get(), t as bool?, response => User = response.Data);
            });
        }
    }
}

