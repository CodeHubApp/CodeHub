using System;
using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryForksViewModel : BaseRepositoriesViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public RepositoryForksViewModel(IApplicationService applicationService)
            : base(applicationService)
        {
            Title = "Forks";

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
            {
                var forks = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetForks();
                return RepositoryCollection.SimpleCollectionLoad(forks, t as bool?);
            });
        }
    }
}

