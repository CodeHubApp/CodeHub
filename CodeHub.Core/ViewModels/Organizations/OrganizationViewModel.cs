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

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationViewModel : BaseViewModel, ILoadableViewModel
	{
        private UserModel _userModel;

        public string Name { get; set; }

        public UserModel Organization
        {
            get { return _userModel; }
            private set { this.RaiseAndSetIfChanged(ref _userModel, value); }
        }

        public IReactiveCommand<object> GoToMembersCommand { get; private set; }

        public IReactiveCommand<object> GoToTeamsCommand { get; private set; }

        public IReactiveCommand<object> GoToFollowersCommand { get; private set; }

        public IReactiveCommand<object> GoToEventsCommand { get; private set; }

        public IReactiveCommand<object> GoToGistsCommand { get; private set; }

        public IReactiveCommand<object> GoToRepositoriesCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public OrganizationViewModel(IApplicationService applicationService)
        {
            GoToMembersCommand = ReactiveCommand.Create();
            GoToMembersCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<OrganizationMembersViewModel>();
                vm.OrganizationName = Name;
                ShowViewModel(vm);
            });

            GoToTeamsCommand = ReactiveCommand.Create();
            GoToTeamsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<TeamsViewModel>();
                vm.OrganizationName = Name;
                ShowViewModel(vm);
            });

            GoToFollowersCommand = ReactiveCommand.Create();
            GoToFollowersCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserFollowersViewModel>();
                vm.Username = Name;
                ShowViewModel(vm);
            });

            GoToEventsCommand = ReactiveCommand.Create();
            GoToEventsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserEventsViewModel>();
                vm.Username = Name;
                ShowViewModel(vm);
            });

            GoToGistsCommand = ReactiveCommand.Create();
            GoToGistsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserGistsViewModel>();
                vm.Username = Name;
                ShowViewModel(vm);
            });

            GoToRepositoriesCommand = ReactiveCommand.Create();
            GoToRepositoriesCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<OrganizationRepositoriesViewModel>();
                vm.Name = Name;
                ShowViewModel(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                this.RequestModel(applicationService.Client.Organizations[Name].Get(), t as bool?,
                    response => Organization = response.Data));
        }
	}
}

