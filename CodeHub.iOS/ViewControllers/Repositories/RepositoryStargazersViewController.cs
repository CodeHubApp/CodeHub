using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.ViewControllers.Users;

namespace CodeHub.iOS.ViewControllers.Repositories
{
    public class RepositoryStargazersViewController : BaseUserCollectionViewController<RepositoryStargazersViewModel>
    {
        public RepositoryStargazersViewController()
            : base("There are no stargazers.")
        {
        }
    }
}

