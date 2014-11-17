using System;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.Users;
using CodeHub.Core.ViewModels.Teams;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
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

        private Octokit.Organization _userModel;
        public Octokit.Organization Organization
        {
            get { return _userModel; }
            private set { this.RaiseAndSetIfChanged(ref _userModel, value); }
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

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
                Organization = await _applicationService.GitHubClient.Organization.Get(Username));
        }
	}
}

