using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesWatchedViewModel : BaseRepositoriesViewModel
    {
        private readonly IApplicationService _applicationService;

        public RepositoriesWatchedViewModel(IApplicationService applicationService) 
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

