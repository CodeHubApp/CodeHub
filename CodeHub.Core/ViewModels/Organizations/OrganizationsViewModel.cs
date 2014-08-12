using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationsViewModel : BaseViewModel, ILoadableViewModel
	{
        private readonly ReactiveList<BasicUserModel> _organizations = new ReactiveList<BasicUserModel>();

        public IReadOnlyReactiveList<BasicUserModel> Organizations { get; private set; }

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
            Organizations = _organizations.CreateDerivedCollection(x => x,
                x => x.Login.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            GoToOrganizationCommand = ReactiveCommand.Create();
            GoToOrganizationCommand.OfType<BasicUserModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<OrganizationViewModel>();
                vm.Name = x.Login;
                ShowViewModel(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                _organizations.SimpleCollectionLoad(applicationService.Client.Users[Username].GetOrganizations(),
                    t as bool?));
        }
	}
}

