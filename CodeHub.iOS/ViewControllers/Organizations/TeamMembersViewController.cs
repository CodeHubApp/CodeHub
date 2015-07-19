using CodeHub.iOS.ViewControllers.Users;
using CodeHub.Core.ViewModels.Organizations;

namespace CodeHub.iOS.ViewControllers.Organizations
{
    public class TeamMembersViewController : BaseUserCollectionViewController<TeamMembersViewModel>
    {
        public TeamMembersViewController()
            : base("There are no team members.")
        {
        }
    }
}