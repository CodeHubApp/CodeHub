using System.Threading.Tasks;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class TeamsViewModel : LoadableViewModel
    {
        public ReactiveList<Octokit.Team> Teams { get; } = new ReactiveList<Octokit.Team>();

        public string OrganizationName { get; }

        public TeamsViewModel(string organizationName)
        {
            OrganizationName = organizationName;
            Title = "Teams";
        }

        protected override async Task Load()
        {
            var result = await this.GetApplication().GitHubClient.Organization.Team.GetAll(OrganizationName);
            Teams.Reset(result);
        }
    }
}