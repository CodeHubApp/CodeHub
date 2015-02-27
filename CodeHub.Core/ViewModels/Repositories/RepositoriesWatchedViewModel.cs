using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesWatchedViewModel : BaseRepositoriesViewModel
    {
        private readonly ISessionService _applicationService;

        public RepositoriesWatchedViewModel(ISessionService applicationService) 
            : base(applicationService)
        {
            _applicationService = applicationService;
            Title = "Watched";
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.RepositoryModel>> CreateRequest()
        {
            return _applicationService.Client.AuthenticatedUser.Repositories.GetWatching();
        }
    }
}

