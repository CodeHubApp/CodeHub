namespace CodeHub.iOS.Views.User
{
    public class OrganizationMembersView : BaseUserCollectionView
    {
        public override void ViewDidLoad()
        {
            Title = "Members";
            NoItemsText = "No Members";

            base.ViewDidLoad();
        }
    }
}

