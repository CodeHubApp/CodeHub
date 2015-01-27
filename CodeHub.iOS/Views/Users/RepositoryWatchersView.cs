using CodeHub.Core.ViewModels.Users;

namespace CodeHub.iOS.Views.Users
{
    public class RepositoryWatchersView : BaseUserCollectionView<RepositoryWatchersViewModel>
    {
        public RepositoryWatchersView()
            : base("There are no watchers.")
        {
        }
    }
}

