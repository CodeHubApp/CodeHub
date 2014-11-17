using CodeHub.Core.Services;
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

        public IReactiveCommand LoadCommand { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public OrganizationsViewModel(IApplicationService applicationService)
        {
            Title = "Organizations";

            var organizations = new ReactiveList<Octokit.Organization>();
            Organizations = organizations.CreateDerivedCollection(
                x => new UserItemViewModel(x.Login, x.AvatarUrl, true, () =>
                {
                    var vm = CreateViewModel<OrganizationViewModel>();
                    vm.Username = x.Name;
                    ShowViewModel(vm);
                }),
                x => x.Login.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
                organizations.Reset(await applicationService.GitHubClient.Organization.GetAll(Username)));
        }
	}
}

