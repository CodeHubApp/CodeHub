using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels;
using CodeHub.Core.ViewModels.User;
using GitHubSharp.Models;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Teams
{
    public class TeamsViewModel : LoadableViewModel
    {
        private readonly CollectionViewModel<TeamShortModel> _teams = new CollectionViewModel<TeamShortModel>();

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

        public void Init(NavObject navObject)
        {
            OrganizationName = navObject.Name;
        }

        protected override Task Load(bool forceDataRefresh)
        {
			return Teams.SimpleCollectionLoad(this.GetApplication().Client.Organizations[OrganizationName].GetTeams(), forceDataRefresh);
        }

        public class NavObject
        {
            public string Name { get; set; }
        }
    }
}