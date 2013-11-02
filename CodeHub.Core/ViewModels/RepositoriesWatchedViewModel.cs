using GitHubSharp.Models;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels
{
    public class RepositoriesWatchedViewModel : RepositoriesViewModel
    {
        public RepositoriesWatchedViewModel()
            : base(string.Empty)
        {
        }

        public override Task Load(bool forceDataRefresh)
        {
            return Repositories.SimpleCollectionLoad(Application.Client.AuthenticatedUser.Repositories.GetWatching(), forceDataRefresh);
        }
    }
}

