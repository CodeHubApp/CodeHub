using CodeHub.Core.ViewModels.Users;

namespace CodeHub.iOS.Views.Users
{
    public class UserFollowersView : BaseUserCollectionView<UserFollowersViewModel>
    {
        public UserFollowersView()
            : base("There are no followers.")
        {
        }
    }
}

