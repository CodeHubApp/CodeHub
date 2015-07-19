using CodeHub.iOS.ViewControllers.Users;
using CodeHub.Core.ViewModels.Repositories;

namespace CodeHub.iOS.ViewControllers.Repositories
{
    public class RepositoryWatchersViewController : BaseUserCollectionViewController<RepositoryWatchersViewModel>
    {
        public RepositoryWatchersViewController()
            : base("There are no watchers.")
        {
        }
    }
}

