using CodeHub.iOS.Views.Users;
using CodeHub.Core.ViewModels.Users;

namespace CodeHub.iOS.Views.Users
{
    public class TeamMembersView : BaseUserCollectionView<TeamMembersViewModel>
    {
        public TeamMembersView()
            : base("Members")
        {
        }
    }
}