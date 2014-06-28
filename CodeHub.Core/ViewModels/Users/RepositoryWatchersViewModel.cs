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
	        LoadCommand.RegisterAsyncTask(t =>
                Users.SimpleCollectionLoad(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetWatchers(), t as bool?));
	    }
    }
}

