using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Users;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
using Xamarin.Utilities.Core;

namespace CodeHub.Core.ViewModels.Teams
{
    public class TeamsViewModel : BaseViewModel, ILoadableViewModel, IProvidesSearchKeyword
    {
        public IReadOnlyReactiveList<TeamItemViewModel> Teams { get; private set; }

        public string OrganizationName { get; set; }

        public IReactiveCommand LoadCommand { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public TeamsViewModel(IApplicationService applicationService)
        {
            Title = "Teams";

            var teams = new ReactiveList<Octokit.Team>();
            Teams = teams.CreateDerivedCollection(
                x => new TeamItemViewModel(x.Name, () => 
                {
                    var vm = CreateViewModel<TeamMembersViewModel>();
                    vm.Id = x.Id;
                    ShowViewModel(vm);
                }),
                x => x.Name.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ => 
                teams.Reset(await applicationService.GitHubClient.Organization.Team.GetAll(OrganizationName)));
        }
    }
}