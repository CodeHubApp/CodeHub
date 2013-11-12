namespace CodeHub.iOS.Views.User
{
    public class OrganizationMembersView : BaseUserCollectionView
    {
        public override void ViewDidLoad()
        {
            Title = "Members".t();
            SearchPlaceholder = "Search Memebers".t();
            NoItemsText = "No Members".t();

            base.ViewDidLoad();
        }
    }
}

