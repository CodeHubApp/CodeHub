using CodeHub.iOS.ViewControllers.Users;

namespace CodeHub.iOS.ViewControllers.Teams
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