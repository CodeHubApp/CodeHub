using CodeHub.Core.Services;
using ReactiveUI;
using CodeHub.Core.ViewModels.Users;
using System;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationsViewModel : BaseViewModel, ILoadableViewModel, IProvidesSearchKeyword
	{
        public IReadOnlyReactiveList<UserItemViewModel> Organizations { get; private set; }

        public string Username { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public OrganizationsViewModel(ISessionService applicationService)
        {
            Title = "Organizations";

            var organizations = new ReactiveList<Octokit.Organization>();
            Organizations = organizations.CreateDerivedCollection(
                x => new UserItemViewModel(x.Login, x.AvatarUrl, true, () => {
                    var vm = this.CreateViewModel<OrganizationViewModel>();
                    vm.Init(x.Login, x);
                    NavigateTo(vm);
                }),
                filter: x => x.Name.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
                organizations.Reset(await applicationService.GitHubClient.Organization.GetAll(Username)));
        }

        public OrganizationsViewModel Init(string username)
        {
            Username = username;
            return this;
        }
	}
}

