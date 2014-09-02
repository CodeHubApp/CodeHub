using System;
using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class UserRepositoriesViewModel : BaseRepositoriesViewModel
    {
        public string Username { get; set; }

        public UserRepositoriesViewModel(IApplicationService applicationService)
            : base(applicationService)
        {
            ShowRepositoryOwner = false;
            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
            {
                var request = string.Equals(applicationService.Account.Username, Username, StringComparison.OrdinalIgnoreCase) ? 
                    applicationService.Client.AuthenticatedUser.Repositories.GetAll() : 
                    applicationService.Client.Users[Username].Repositories.GetAll();
                return RepositoryCollection.SimpleCollectionLoad(request, t as bool?);
            });
        }
    }
}
