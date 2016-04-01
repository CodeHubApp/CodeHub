using CodeHub.iOS.ViewControllers.Users;

namespace CodeHub.iOS.ViewControllers.Organizations
{
    public class TeamMembersViewController : BaseUserCollectionViewController
    {
        public TeamMembersViewController()
            : base("There are no team members.")
        {
            Title = "Members";
        }
    }
}