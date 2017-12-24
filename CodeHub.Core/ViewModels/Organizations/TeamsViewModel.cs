using System.Threading.Tasks;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class TeamsViewModel : LoadableViewModel
    {
        private readonly IApplicationService _applicationService;

        public CollectionViewModel<Octokit.Team> Teams { get; } = new CollectionViewModel<Octokit.Team>();

        public string OrganizationName { get; private set; }

        public TeamsViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;

            Title = "Teams";
        }

        public void Init(NavObject navObject)
        {
            OrganizationName = navObject.Name;
        }

        protected override async Task Load()
        {
            var teams = await _applicationService.GitHubClient.Organization.Team.GetAll(OrganizationName);
            Teams.Items.Reset(teams);
        }

        public class NavObject
        {
            public string Name { get; set; }
        }
    }
}