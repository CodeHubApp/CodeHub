using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesWatchedViewModel : RepositoriesViewModel
    {
        public RepositoriesWatchedViewModel()
        {
            ShowRepositoryOwner = true;
        }

        protected override Task Load()
        {
            return Repositories.SimpleCollectionLoad(this.GetApplication().Client.AuthenticatedUser.Repositories.GetWatching());
        }
    }
}

