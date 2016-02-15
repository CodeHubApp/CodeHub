namespace CodeHub.iOS.Views.User
{
    public class UserFollowersView : BaseUserCollectionView
    {
        public override void ViewDidLoad()
        {
            Title = "Followers";
            NoItemsText = "No Followers";
            base.ViewDidLoad();
        }
    }
}

