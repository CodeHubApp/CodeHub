namespace CodeHub.iOS.Views.User
{
    public class UserFollowersView : BaseUserCollectionView
    {
        public override void ViewDidLoad()
        {
            Title = "Followers".t();
            SearchPlaceholder = "Search Followers".t();
            NoItemsText = "No Followers".t();

            base.ViewDidLoad();
        }
    }
}

