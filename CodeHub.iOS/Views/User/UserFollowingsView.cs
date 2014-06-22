using CodeHub.Core.ViewModels.User;

namespace CodeHub.iOS.Views.User
{
    public class UserFollowingsView : BaseUserCollectionView<UserFollowingsViewModel>
    {
        public override void ViewDidLoad()
        {
            Title = "Following";
            NoItemsText = "Not Following Anyone";

            base.ViewDidLoad();
        }
    }
}

