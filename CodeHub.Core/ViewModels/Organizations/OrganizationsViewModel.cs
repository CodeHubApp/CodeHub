using CodeHub.Core.Services;
using ReactiveUI;
using CodeHub.Core.ViewModels.Users;
using System;
using Octokit;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationsViewModel : BaseSearchableListViewModel<Organization, UserItemViewModel>
	{
        public string Username { get; private set; }

        public OrganizationsViewModel(ISessionService applicationService)
        {
            Title = "Organizations";

            Items = InternalItems.CreateDerivedCollection(
                x => new UserItemViewModel(x.Login, x.AvatarUrl, true, () => {
                    var vm = this.CreateViewModel<OrganizationViewModel>();
                    vm.Init(x.Login, x);
                    NavigateTo(vm);
                }),
                filter: x => x.Name.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
                InternalItems.Reset(await applicationService.GitHubClient.Organization.GetAll(Username)));
        }

        public OrganizationsViewModel Init(string username)
        {
            Username = username;
            return this;
        }
	}
}

