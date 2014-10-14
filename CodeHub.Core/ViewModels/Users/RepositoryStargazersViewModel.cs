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
            Title = "Stargazers";
            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                Load(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetStargazers(), t as bool?));
	    }
    }
}

