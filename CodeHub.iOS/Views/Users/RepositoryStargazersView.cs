using CodeHub.Core.ViewModels.Users;

namespace CodeHub.iOS.Views.Users
{
    public class RepositoryStargazersView : BaseUserCollectionView<RepositoryStargazersViewModel>
    {
        public RepositoryStargazersView()
            : base("There are no stargazers.")
        {
        }
    }
}

