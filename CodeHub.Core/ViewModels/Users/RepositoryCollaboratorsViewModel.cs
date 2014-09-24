using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Users
{
    public class RepositoryCollaboratorsViewModel : BaseUserCollectionViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public RepositoryCollaboratorsViewModel(IApplicationService applicationService)
        {
            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                Load(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetCollaborators(), t as bool?));
        }
    }
}

