using CodeHub.Core.ViewModels.User;

namespace CodeHub.iOS.Views.User
{
    public class UserFollowersView : BaseUserCollectionView<UserFollowersViewModel>
    {
        public override void ViewDidLoad()
        {
            Title = "Followers";
            NoItemsText = "No Followers";

            base.ViewDidLoad();
        }
    }
}

