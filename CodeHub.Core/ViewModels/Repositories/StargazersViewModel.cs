using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.User;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
	public class StargazersViewModel : BaseUserCollectionViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

	    public StargazersViewModel(IApplicationService applicationService)
	    {
	        LoadCommand.RegisterAsyncTask(t =>
                Users.SimpleCollectionLoad(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetStargazers(), t as bool?));
	    }
    }
}

