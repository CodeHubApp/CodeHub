using CodeHub.Core.ViewModels.Users;

namespace CodeHub.iOS.Views.Users
{
    public class UserFollowingsView : BaseUserCollectionView<UserFollowingsViewModel>
    {
        public UserFollowingsView()
            : base("There are no followers.")
        {
        }
    }
}

