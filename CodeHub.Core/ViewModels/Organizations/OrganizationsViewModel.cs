using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.ReactiveAddons;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationsViewModel : BaseViewModel, ILoadableViewModel
	{
        public ReactiveCollection<BasicUserModel> Organizations { get; private set; }

        public string Username { get; set; }

        public IReactiveCommand<object> GoToOrganizationCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public OrganizationsViewModel(IApplicationService applicationService)
        {
            Organizations = new ReactiveCollection<BasicUserModel>();

            GoToOrganizationCommand = ReactiveCommand.Create();
            GoToOrganizationCommand.OfType<BasicUserModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<OrganizationViewModel>();
                vm.Name = x.Login;
                ShowViewModel(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                Organizations.SimpleCollectionLoad(applicationService.Client.Users[Username].GetOrganizations(),
                    t as bool?));
        }
	}
}

