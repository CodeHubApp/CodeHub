using CodeHub.iOS.ViewControllers.Users;
using CodeHub.Core.ViewModels.Organizations;

namespace CodeHub.iOS.ViewControllers.Organizations
{
    public class OrganizationMembersViewController : BaseUserCollectionViewController<OrganizationMembersViewModel>
    {
        public OrganizationMembersViewController()
            : base("There are no members.")
        {
        }
    }
}

