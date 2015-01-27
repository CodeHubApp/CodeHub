using CodeHub.Core.ViewModels.Users;

namespace CodeHub.iOS.Views.Users
{
    public class OrganizationMembersView : BaseUserCollectionView<OrganizationMembersViewModel>
    {
        public OrganizationMembersView()
            : base("There are no members.")
        {
        }
    }
}

