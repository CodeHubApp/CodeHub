using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.ReactiveAddons;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationsViewModel : LoadableViewModel
	{
        public ReactiveCollection<BasicUserModel> Organizations { get; private set; }

        public string Username { get; set; }

        public IReactiveCommand GoToOrganizationCommand { get; private set; }

        public OrganizationsViewModel(IApplicationService applicationService)
        {
            Organizations = new ReactiveCollection<BasicUserModel>();

            GoToOrganizationCommand = new ReactiveCommand();
            GoToOrganizationCommand.OfType<BasicUserModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<OrganizationViewModel>();
                vm.Name = x.Login;
                ShowViewModel(vm);
            });

            LoadCommand.RegisterAsyncTask(t =>
                Organizations.SimpleCollectionLoad(applicationService.Client.Users[Username].GetOrganizations(),
                    t as bool?));
        }
	}
}

