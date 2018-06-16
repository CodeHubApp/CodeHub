using System.Threading.Tasks;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationViewModel : LoadableViewModel
    {
        public string Name { get; }

        private Octokit.Organization _organization;
        public Octokit.Organization Organization
        {
            get { return _organization; }
            private set { this.RaiseAndSetIfChanged(ref _organization, value); }
        }

        public OrganizationViewModel(string name)
        {
            Name = name;
        }

        protected override async Task Load()
        {
            Organization = await this.GetApplication().GitHubClient.Organization.Get(Name);
        }
    }
}

