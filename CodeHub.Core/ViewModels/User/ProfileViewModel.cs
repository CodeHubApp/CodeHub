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

namespace CodeHub.Core.ViewModels.User
{
    public class ProfileViewModel : LoadableViewModel
    {
        private readonly IApplicationService _applicationService;

        public string Username { get; set; }

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

        public IReactiveCommand GoToFollowersCommand { get; private set; }

        public IReactiveCommand GoToFollowingCommand { get; private set; }

        public IReactiveCommand GoToEventsCommand { get; private set; }

        public IReactiveCommand GoToOrganizationsCommand  { get; private set; }

        public IReactiveCommand GoToRepositoriesCommand { get; private set; }

        public IReactiveCommand GoToGistsCommand { get; private set; }

		public IReactiveCommand ToggleFollowingCommand { get; private set; }


		private async Task ToggleFollowing()
		{
		    if (!IsFollowing.HasValue) return;
			if (IsFollowing.Value)
				await _applicationService.Client.ExecuteAsync(_applicationService.Client.AuthenticatedUser.Unfollow(Username));
			else
				await _applicationService.Client.ExecuteAsync(_applicationService.Client.AuthenticatedUser.Follow(Username));
			IsFollowing = !IsFollowing.Value;
		}

        public ProfileViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;

            ToggleFollowingCommand = new ReactiveCommand(this.WhenAnyValue(x => x.IsFollowing, x => x.HasValue));
            ToggleFollowingCommand.RegisterAsyncTask(t => ToggleFollowing());

            GoToGistsCommand = new ReactiveCommand();
            GoToGistsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserGistsViewModel>();
                vm.Username = Username;
                ShowViewModel(vm);
            });

            GoToRepositoriesCommand = new ReactiveCommand();
            GoToRepositoriesCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserRepositoriesViewModel>();
                vm.Username = Username;
                ShowViewModel(vm);
            });

            GoToOrganizationsCommand = new ReactiveCommand();
            GoToOrganizationsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<OrganizationsViewModel>();
                vm.Username = Username;
                ShowViewModel(vm);
            });

            GoToEventsCommand = new ReactiveCommand();
            GoToEventsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserEventsViewModel>();
                vm.Username = Username;
                ShowViewModel(vm);
            });

            GoToFollowingCommand = new ReactiveCommand();
            GoToFollowingCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserFollowingsViewModel>();
                vm.Username = Username;
                ShowViewModel(vm);
            });

            GoToFollowersCommand = new ReactiveCommand();
            GoToFollowersCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserFollowersViewModel>();
                vm.Username = Username;
                ShowViewModel(vm);
            });

            LoadCommand.RegisterAsyncTask(t =>
            {
                this.RequestModel(applicationService.Client.AuthenticatedUser.IsFollowing(Username), t as bool?, x => IsFollowing = x.Data).FireAndForget();
                return this.RequestModel(applicationService.Client.Users[Username].Get(), t as bool?, response => User = response.Data);
            });
        }
    }
}

