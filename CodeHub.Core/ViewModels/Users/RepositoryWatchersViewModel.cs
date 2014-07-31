using CodeHub.Core.Services;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Users
{
    public class RepositoryWatchersViewModel : BaseUserCollectionViewModel, ILoadableViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public RepositoryWatchersViewModel(IApplicationService applicationService)
	    {
            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                Users.SimpleCollectionLoad(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetWatchers(), t as bool?));
	    }
    }
}

