using CodeHub.Core.Services;
using ReactiveUI;
using System;
using Octokit;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class TeamsViewModel : BaseSearchableListViewModel<Team, TeamItemViewModel>
    {
        public string OrganizationName { get; private set; }

        public TeamsViewModel(ISessionService applicationService)
        {
            Title = "Teams";

            Items = InternalItems.CreateDerivedCollection(
                x => new TeamItemViewModel(x.Name, () => 
                {
                    var vm = this.CreateViewModel<TeamMembersViewModel>();
                    vm.Init(x.Id);
                    NavigateTo(vm);
                }),
                filter: x => x.Name.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ => 
                InternalItems.Reset(await applicationService.GitHubClient.Organization.Team.GetAll(OrganizationName)));
        }

        public TeamsViewModel Init(string organizationName)
        {
            OrganizationName = organizationName;
            return this;
        }
    }
}