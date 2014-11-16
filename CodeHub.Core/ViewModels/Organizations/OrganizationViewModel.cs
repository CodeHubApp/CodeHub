using System;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.Users;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.Teams;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationViewModel : BaseViewModel, ILoadableViewModel
	{
        private readonly IApplicationService _applicationService;

        private string _username;
        public string Username
        {
            get { return _username; }
            set { this.RaiseAndSetIfChanged(ref _username, value); }
        }

        private UserModel _userModel;
        public UserModel Organization
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

        public IReactiveCommand GoToMembersCommand { get; private set; }

        public IReactiveCommand GoToTeamsCommand { get; private set; }

        public IReactiveCommand GoToFollowersCommand { get; private set; }

        public IReactiveCommand GoToFollowingCommand { get; private set; }

        public IReactiveCommand GoToEventsCommand { get; private set; }

        public IReactiveCommand GoToGistsCommand { get; private set; }

        public IReactiveCommand GoToRepositoriesCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public IReactiveCommand ToggleFollowingCommand { get; private set; }

        public OrganizationViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;

            this.WhenAnyValue(x => x.Organization, x => x.Username, 
                (x, y) => x == null ? y : (string.IsNullOrEmpty(x.Name) ? x.Login : x.Name))
                .Select(x => x ?? "Organization")
                .Subscribe(x => Title = x);

            GoToMembersCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = CreateViewModel<OrganizationMembersViewModel>();
                vm.OrganizationName = Username;
                ShowViewModel(vm);
            });

            GoToTeamsCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = CreateViewModel<TeamsViewModel>();
                vm.OrganizationName = Username;
                ShowViewModel(vm);
            });

            GoToFollowersCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = CreateViewModel<UserFollowersViewModel>();
                vm.Username = Username;
                ShowViewModel(vm);
            });

            GoToFollowingCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = CreateViewModel<UserFollowingsViewModel>();
                vm.Username = Username;
                ShowViewModel(vm);
            });

            GoToEventsCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = CreateViewModel<UserEventsViewModel>();
                vm.Username = Username;
                ShowViewModel(vm);
            });

            GoToGistsCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = CreateViewModel<UserGistsViewModel>();
                vm.Username = Username;
                ShowViewModel(vm);
            });

            GoToRepositoriesCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = CreateViewModel<OrganizationRepositoriesViewModel>();
                vm.Name = Username;
                ShowViewModel(vm);
            });

            ToggleFollowingCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.IsFollowing).Select(x => x.HasValue), 
                async _ => 
                {
                    if (!IsFollowing.HasValue) return;
                    if (IsFollowing.Value)
                        await _applicationService.Client.ExecuteAsync(_applicationService.Client.AuthenticatedUser.Unfollow(Username));
                    else
                        await _applicationService.Client.ExecuteAsync(_applicationService.Client.AuthenticatedUser.Follow(Username));
                    IsFollowing = !IsFollowing.Value;
                });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
            {
                this.RequestModel(applicationService.Client.AuthenticatedUser.IsFollowing(Username), t as bool?, x => IsFollowing = x.Data).FireAndForget();
                return this.RequestModel(applicationService.Client.Organizations[Username].Get(), t as bool?, response => Organization = response.Data);
            });
        }
	}
}

