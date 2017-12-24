using System.Threading.Tasks;
using System.Windows.Input;
using CodeHub.Core.Services;
using MvvmCross.Core.ViewModels;
using Splat;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationsViewModel : LoadableViewModel
    {
        private readonly IApplicationService _applicationService;

        public CollectionViewModel<Octokit.Organization> Organizations { get; } = new CollectionViewModel<Octokit.Organization>();

        public string Username { get; private set; }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
        }

        public OrganizationsViewModel(
            IApplicationService applicationService = null)
        {
            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();

            Title = "Organizations";
        }

        public ICommand GoToOrganizationCommand
        {
            get { return new MvxCommand<Octokit.Organization>(x => ShowViewModel<OrganizationViewModel>(new OrganizationViewModel.NavObject { Name = x.Login }));}
        }

        protected override async Task Load()
        {
            var organizations = await _applicationService.GitHubClient.Organization.GetAllForUser(Username);
            Organizations.Items.Reset(organizations);
        }

        public class NavObject
        {
            public string Username { get; set; }
        }
    }
}

