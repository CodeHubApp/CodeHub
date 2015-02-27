using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryForksViewModel : BaseRepositoriesViewModel
    {
        private readonly ISessionService _applicationService;

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public RepositoryForksViewModel(ISessionService applicationService)
            : base(applicationService)
        {
            _applicationService = applicationService;
            Title = "Forks";
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.RepositoryModel>> CreateRequest()
        {
            return _applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetForks();
        }
    }
}

