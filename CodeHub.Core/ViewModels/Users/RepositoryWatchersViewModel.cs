using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Users
{
    public class RepositoryWatchersViewModel : BaseUserCollectionViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public RepositoryWatchersViewModel(IApplicationService applicationService)
	    {
            Title = "Watchers";
            LoadCommand = ReactiveCommand.CreateAsyncTask(t => 
                Load(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetWatchers(), t as bool?));
	    }
    }
}

