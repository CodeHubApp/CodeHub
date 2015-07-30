using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Users;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryStargazersViewModel : BaseUsersViewModel
    {
        private readonly ISessionService _sessionService;

        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

	    public RepositoryStargazersViewModel(ISessionService applicationService)
	    {
            _sessionService = applicationService;
            Title = "Stargazers";
	    }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.BasicUserModel>> CreateRequest()
        {
            return _sessionService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetStargazers();
        }

        public RepositoryStargazersViewModel Init(string repositoryOwner, string repositoryName)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            return this;
        }
    }
}

