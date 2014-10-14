using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
using CodeHub.Core.ViewModels.Users;
using Xamarin.Utilities.Core;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationsViewModel : BaseViewModel, ILoadableViewModel, IProvidesSearchKeyword
	{
        public IReadOnlyReactiveList<UserItemViewModel> Organizations { get; private set; }

        public string Username { get; set; }

        public IReactiveCommand<object> GoToOrganizationCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public OrganizationsViewModel(IApplicationService applicationService)
        {
            var organizations = new ReactiveList<BasicUserModel>();
            Organizations = organizations.CreateDerivedCollection(
                x => new UserItemViewModel(x.Login, x.AvatarUrl, true, GoToOrganizationCommand.ExecuteIfCan),
                x => x.Login.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            GoToOrganizationCommand = ReactiveCommand.Create();
            GoToOrganizationCommand.OfType<UserItemViewModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<OrganizationViewModel>();
                vm.Username = x.Name;
                ShowViewModel(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                organizations.SimpleCollectionLoad(applicationService.Client.Users[Username].GetOrganizations(),
                    t as bool?));
        }
	}
}

