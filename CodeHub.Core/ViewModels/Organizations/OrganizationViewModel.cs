using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationViewModel : LoadableViewModel
    {
        private readonly IApplicationService _applicationService;

        private Octokit.Organization _userModel;

        public string Name { get; private set; }

        public OrganizationViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        public void Init(NavObject navObject)
        {
            Name = navObject.Name;
        }

        public Octokit.Organization Organization
        {
            get { return _userModel; }
            private set { this.RaiseAndSetIfChanged(ref _userModel, value); }
        }

        public ICommand GoToTeamsCommand
        {
            get { return new MvxCommand(() => ShowViewModel<TeamsViewModel>(new TeamsViewModel.NavObject { Name = Name })); }
        }

        public ICommand GoToEventsCommand
        {
            get { return new MvxCommand(() => ShowViewModel<UserEventsViewModel>(new UserEventsViewModel.NavObject { Username = Name })); }
        }

        protected override async Task Load()
        {
            Organization = await _applicationService.GitHubClient.Organization.Get(Name);
        }

        public class NavObject
        {
            public string Name { get; set; }
        }
    }
}

