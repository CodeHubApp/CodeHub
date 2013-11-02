using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels
{
    public class TeamsViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly CollectionViewModel<TeamShortModel> _teams = new CollectionViewModel<TeamShortModel>();
        private readonly IApplicationService _application;

        public CollectionViewModel<TeamShortModel> Teams
        {
            get { return _teams; }
        }

        public string OrganizationName
        {
            get;
            private set;
        }

        public ICommand GoToTeamCommand
        {
            get { return new MvxCommand<TeamShortModel>(x => ShowViewModel<TeamMembersViewModel>(new TeamMembersViewModel.NavObject { Id = x.Id })); }
        }

        public TeamsViewModel(IApplicationService application)
        {
            _application = application;
        }

        public void Init(NavObject navObject)
        {
            OrganizationName = navObject.Name;
        }

        public Task Load(bool forceDataRefresh)
        {
            return Teams.SimpleCollectionLoad(_application.Client.Organizations[OrganizationName].GetTeams(), forceDataRefresh);
        }

        public class NavObject
        {
            public string Name { get; set; }
        }
    }
}