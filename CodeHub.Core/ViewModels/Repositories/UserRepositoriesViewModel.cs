using System;
using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class UserRepositoriesViewModel : RepositoriesViewModel
    {
        public string Username { get; set; }

        public UserRepositoriesViewModel(IApplicationService applicationService)
            : base(applicationService)
        {
            LoadCommand.RegisterAsyncTask(t =>
            {
                var request = string.Equals(applicationService.Account.Username, Username, StringComparison.OrdinalIgnoreCase) ? 
                    applicationService.Client.AuthenticatedUser.Repositories.GetAll() : 
                    applicationService.Client.Users[Username].Repositories.GetAll();
                return Repositories.SimpleCollectionLoad(request, t as bool?);
            });
        }
    }
}
