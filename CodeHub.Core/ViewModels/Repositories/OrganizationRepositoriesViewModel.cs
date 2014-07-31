using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class OrganizationRepositoriesViewModel : RepositoriesViewModel
    {
        public string Name { get; set; }

        public OrganizationRepositoriesViewModel(IApplicationService applicationService)
            : base(applicationService)
        {
            ShowRepositoryOwner = false;
            LoadCommand = ReactiveCommand.CreateAsyncTask(t => 
                Repositories.SimpleCollectionLoad(applicationService.Client.Organizations[Name].Repositories.GetAll(), t as bool?));
        }
    }
}

