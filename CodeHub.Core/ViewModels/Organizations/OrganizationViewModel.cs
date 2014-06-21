using System;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.User;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.Teams;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationViewModel : LoadableViewModel
	{
        private UserModel _userModel;

        public string Name { get; set; }

        public UserModel Organization
        {
            get { return _userModel; }
            private set { this.RaiseAndSetIfChanged(ref _userModel, value); }
        }

        public IReactiveCommand GoToMembersCommand { get; private set; }

        public IReactiveCommand GoToTeamsCommand { get; private set; }

        public IReactiveCommand GoToFollowersCommand { get; private set; }

        public IReactiveCommand GoToEventsCommand { get; private set; }

        public IReactiveCommand GoToGistsCommand { get; private set; }

        public IReactiveCommand GoToRepositoriesCommand { get; private set; }

        public OrganizationViewModel(IApplicationService applicationService)
        {
            GoToMembersCommand = new ReactiveCommand();
            GoToMembersCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<OrganizationMembersViewModel>();
                vm.OrganizationName = Name;
                ShowViewModel(vm);
            });

            GoToTeamsCommand = new ReactiveCommand();
            GoToTeamsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<TeamsViewModel>();
                vm.OrganizationName = Name;
                ShowViewModel(vm);
            });

            GoToFollowersCommand = new ReactiveCommand();
            GoToFollowersCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserFollowersViewModel>();
                vm.Username = Name;
                ShowViewModel(vm);
            });

            GoToEventsCommand = new ReactiveCommand();
            GoToEventsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserEventsViewModel>();
                vm.Username = Name;
                ShowViewModel(vm);
            });

            GoToGistsCommand = new ReactiveCommand();
            GoToGistsCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<UserGistsViewModel>();
                vm.Username = Name;
                ShowViewModel(vm);
            });

            GoToRepositoriesCommand = new ReactiveCommand();
            GoToRepositoriesCommand.Subscribe(_ =>
            {
                var vm = CreateViewModel<OrganizationRepositoriesViewModel>();
                vm.Name = Name;
                ShowViewModel(vm);
            });

            LoadCommand.RegisterAsyncTask(t =>
                this.RequestModel(applicationService.Client.Organizations[Name].Get(), t as bool?,
                    response => Organization = response.Data));
        }
	}
}

