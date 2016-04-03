using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels;
using CodeHub.Core.ViewModels.User;
using GitHubSharp.Models;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class TeamsViewModel : LoadableViewModel
    {
        public CollectionViewModel<TeamShortModel> Teams { get; }

        public string OrganizationName { get; private set; }

        public ICommand GoToTeamCommand
        {
            get { return new MvxCommand<TeamShortModel>(x => ShowViewModel<TeamMembersViewModel>(new TeamMembersViewModel.NavObject { Id = x.Id })); }
        }

        public TeamsViewModel()
        {
            Title = "Teams";
            Teams = new CollectionViewModel<TeamShortModel>();
        }

        public void Init(NavObject navObject)
        {
            OrganizationName = navObject.Name;
        }

        protected override Task Load()
        {
            return Teams.SimpleCollectionLoad(this.GetApplication().Client.Organizations[OrganizationName].GetTeams());
        }

        public class NavObject
        {
            public string Name { get; set; }
        }
    }
}