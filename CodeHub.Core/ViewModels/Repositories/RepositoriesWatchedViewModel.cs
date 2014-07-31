using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesWatchedViewModel : RepositoriesViewModel
    {
        public RepositoriesWatchedViewModel(IApplicationService applicationService) : base(applicationService)
        {
            ShowRepositoryOwner = true;

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                Repositories.SimpleCollectionLoad(
                    applicationService.Client.AuthenticatedUser.Repositories.GetWatching(), t as bool?));
        }
    }
}

