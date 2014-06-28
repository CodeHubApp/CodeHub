using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Users
{
	public class RepositoryStargazersViewModel : BaseUserCollectionViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

	    public RepositoryStargazersViewModel(IApplicationService applicationService)
	    {
	        LoadCommand.RegisterAsyncTask(t =>
                Users.SimpleCollectionLoad(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetStargazers(), t as bool?));
	    }
    }
}

