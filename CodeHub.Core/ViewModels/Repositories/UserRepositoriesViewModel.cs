using System;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class UserRepositoriesViewModel : BaseRepositoriesViewModel
    {
        private readonly IApplicationService _applicationService;

        public string Username { get; set; }

        public UserRepositoriesViewModel(IApplicationService applicationService)
            : base(applicationService)
        {
            _applicationService = applicationService;
            ShowRepositoryOwner = false;
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.RepositoryModel>> CreateRequest()
        {
            return string.Equals(_applicationService.Account.Username, Username, StringComparison.OrdinalIgnoreCase) ? 
                _applicationService.Client.AuthenticatedUser.Repositories.GetAll() : 
                _applicationService.Client.Users[Username].Repositories.GetAll();;
        }
    }
}
