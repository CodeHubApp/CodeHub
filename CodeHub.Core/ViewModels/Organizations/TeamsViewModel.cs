using CodeHub.Core.Services;
using ReactiveUI;
using System;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class TeamsViewModel : BaseViewModel, ILoadableViewModel, IProvidesSearchKeyword
    {
        public IReadOnlyReactiveList<TeamItemViewModel> Teams { get; private set; }

        public string OrganizationName { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public TeamsViewModel(ISessionService applicationService)
        {
            Title = "Teams";

            var teams = new ReactiveList<Octokit.Team>();
            Teams = teams.CreateDerivedCollection(
                x => new TeamItemViewModel(x.Name, () => 
                {
                    var vm = this.CreateViewModel<TeamMembersViewModel>();
                    vm.Init(x.Id);
                    NavigateTo(vm);
                }),
                filter: x => x.Name.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ => 
                teams.Reset(await applicationService.GitHubClient.Organization.Team.GetAll(OrganizationName)));
        }

        public TeamsViewModel Init(string organizationName)
        {
            OrganizationName = organizationName;
            return this;
        }
    }
}