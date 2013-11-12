namespace CodeHub.iOS.Views.User
{
    public class UserFollowingsView : BaseUserCollectionView
    {

        public override void ViewDidLoad()
        {
            Title = "Following".t();
            SearchPlaceholder = "Search Following".t();
            NoItemsText = "Not Following Anyone".t();  

            base.ViewDidLoad();
        }
    }
}

