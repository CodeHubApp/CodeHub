using System.Threading.Tasks;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationsViewModel : LoadableViewModel
    {
        public ReactiveList<Octokit.Organization> Organizations { get; } = new ReactiveList<Octokit.Organization>();

        public string Username { get; private set; }

        public OrganizationsViewModel(string username)
        {
            Username = username;
            Title = "Organizations";
        }

        protected override async Task Load()
        {
            var application = this.GetApplication();

            if (Username == application.Account.Username)
            {
                var result = await application.GitHubClient.Organization.GetAllForCurrent();
                Organizations.Reset(result);
            }
            else
            {
                var result = await application.GitHubClient.Organization.GetAllForUser(Username);
                Organizations.Reset(result);
            }
        }
    }
}

