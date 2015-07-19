using CodeHub.Core.ViewModels.Users;

namespace CodeHub.iOS.ViewControllers.Users
{
    public class UserFollowersViewController : BaseUserCollectionViewController<UserFollowersViewModel>
    {
        public UserFollowersViewController()
            : base("There are no followers.")
        {
        }
    }
}

