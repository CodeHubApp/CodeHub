using System;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.Users;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationViewModel : BaseViewModel, ILoadableViewModel
	{
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

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand ToggleFollowingCommand { get; private set; }

        public OrganizationViewModel(IApplicationService applicationService)
        {
            this.WhenAnyValue(x => x.Organization, x => x.Username, 
                (x, y) => x == null ? y : (string.IsNullOrEmpty(x.Name) ? x.Login : x.Name))
                .Select(x => x ?? "Organization")
                .Subscribe(x => Title = x);

            GoToMembersCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<OrganizationMembersViewModel>();
                vm.OrganizationName = Username;
                NavigateTo(vm);
            });

            GoToTeamsCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<TeamsViewModel>();
                vm.OrganizationName = Username;
                NavigateTo(vm);
            });

            GoToFollowersCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<UserFollowersViewModel>();
                vm.Username = Username;
                NavigateTo(vm);
            });

            GoToFollowingCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<UserFollowingsViewModel>();
                vm.Username = Username;
                NavigateTo(vm);
            });

            GoToEventsCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<UserEventsViewModel>();
                vm.Username = Username;
                NavigateTo(vm);
            });

            GoToGistsCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<UserGistsViewModel>();
                vm.Username = Username;
                NavigateTo(vm);
            });

            GoToRepositoriesCommand = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<OrganizationRepositoriesViewModel>();
                vm.Name = Username;
                NavigateTo(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                Organization = await applicationService.GitHubClient.Organization.Get(Username);
            });
        }
	}
}

