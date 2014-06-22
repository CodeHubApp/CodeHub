using CodeHub.Core.ViewModels.User;

namespace CodeHub.iOS.Views.User
{
    public class OrganizationMembersView : BaseUserCollectionView<OrganizationMembersViewModel>
    {
        public override void ViewDidLoad()
        {
            Title = "Members";
            NoItemsText = "No Members";

            base.ViewDidLoad();
        }
    }
}

