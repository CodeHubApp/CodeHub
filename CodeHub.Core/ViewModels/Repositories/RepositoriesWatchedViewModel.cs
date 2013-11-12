using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesWatchedViewModel : RepositoriesViewModel
    {
        public RepositoriesWatchedViewModel()
        {
            ShowRepositoryOwner = true;
        }

        protected override Task Load(bool forceDataRefresh)
        {
            return Repositories.SimpleCollectionLoad(Application.Client.AuthenticatedUser.Repositories.GetWatching(), forceDataRefresh);
        }
    }
}

