using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class OrganizationRepositoriesViewModel : BaseRepositoriesViewModel
    {
        public string Name { get; set; }

        public OrganizationRepositoriesViewModel(IApplicationService applicationService)
            : base(applicationService)
        {
            ShowRepositoryOwner = false;
            LoadCommand = ReactiveCommand.CreateAsyncTask(t => 
                RepositoryCollection.SimpleCollectionLoad(applicationService.Client.Organizations[Name].Repositories.GetAll(), t as bool?));
        }
    }
}

