using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesStarredViewModel : BaseRepositoriesViewModel
    {
        private readonly IApplicationService _applicationService;

        public RepositoriesStarredViewModel(IApplicationService applicationService) 
            : base(applicationService)
        {
            _applicationService = applicationService;
            Title = "Starred";
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.RepositoryModel>> CreateRequest()
        {
            return _applicationService.Client.AuthenticatedUser.Repositories.GetStarred();
        }
    }
}

