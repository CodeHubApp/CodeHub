using CodeHub.Core.ViewModels.Users;

namespace CodeHub.iOS.Views.Users
{
    public class RepositoryContributorsView : BaseUserCollectionView<RepositoryContributorsViewModel>
    {
        public RepositoryContributorsView()
            : base("There are no contributors.")
        {
        }
    }
}

