using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.ViewControllers.Users;

namespace CodeHub.iOS.ViewControllers.Repositories
{
    public class RepositoryContributorsViewController : BaseUserCollectionViewController<RepositoryContributorsViewModel>
    {
        public RepositoryContributorsViewController()
            : base("There are no contributors.")
        {
        }
    }
}

