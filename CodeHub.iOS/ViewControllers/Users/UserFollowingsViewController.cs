using CodeHub.Core.ViewModels.Users;

namespace CodeHub.iOS.ViewControllers.Users
{
    public class UserFollowingsViewController : BaseUserCollectionViewController<UserFollowingsViewModel>
    {
        public UserFollowingsViewController()
            : base("There are no followers.")
        {
        }
    }
}

