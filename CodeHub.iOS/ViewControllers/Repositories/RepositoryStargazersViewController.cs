using CodeHub.iOS.ViewControllers.Users;

namespace CodeHub.iOS.ViewControllers.Repositories
{
    public class RepositoryStargazersViewController : BaseUserCollectionViewController
    {
        public RepositoryStargazersViewController()
            : base("There are no stargazers.")
        {
            Title = "Stargazers";
        }
    }

    public class RepositoryWatchersViewController : BaseUserCollectionViewController
    {
        public RepositoryWatchersViewController()
            : base("There are no watchers.")
        {
            Title = "Watchers";
        }
    }
}

