namespace CodeHub.iOS.Views.User
{
    public class UserFollowingsView : BaseUserCollectionView
    {
        public override void ViewDidLoad()
        {
            Title = "Following";
            NoItemsText = "Not Following Anyone";

            base.ViewDidLoad();
        }
    }
}

