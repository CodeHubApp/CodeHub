using System;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class UserRepositoriesViewModel : BaseRepositoriesViewModel
    {
        private readonly ISessionService _applicationService;

        public string Username { get; private set; }

        public UserRepositoriesViewModel(ISessionService applicationService)
            : base(applicationService)
        {
            _applicationService = applicationService;
            ShowRepositoryOwner = false;
        }

        protected override Uri RepositoryUri
        {
            get
            {
                return string.Equals(_applicationService.Account.Username, Username, StringComparison.OrdinalIgnoreCase) ? 
                    Octokit.ApiUrls.Repositories() : Octokit.ApiUrls.Repositories(Username);
            }
        }

        public UserRepositoriesViewModel Init(string username)
        {
            Username = username;
            return this;
        }
    }
}
