using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesStarredViewModel : BaseRepositoriesViewModel
    {
        public RepositoriesStarredViewModel(IApplicationService applicationService) : base(applicationService)
        {
            Title = "Starred";
            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                RepositoryCollection.SimpleCollectionLoad(
                    applicationService.Client.AuthenticatedUser.Repositories.GetStarred(), t as bool?));
        }
    }
}

