using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesStarredViewModel : RepositoriesViewModel
    {
        public RepositoriesStarredViewModel()
        {
            ShowRepositoryOwner = true;
        }

        protected override Task Load()
        {
            return Repositories.SimpleCollectionLoad(this.GetApplication().Client.AuthenticatedUser.Repositories.GetStarred());
        }
    }
}

