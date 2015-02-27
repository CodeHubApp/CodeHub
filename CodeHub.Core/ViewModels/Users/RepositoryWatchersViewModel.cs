using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Users
{
    public class RepositoryWatchersViewModel : BaseUsersViewModel
    {
        private readonly ISessionService _applicationService;

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public RepositoryWatchersViewModel(ISessionService applicationService)
	    {
            _applicationService = applicationService;
            Title = "Watchers";
	    }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.BasicUserModel>> CreateRequest()
        {
            return _applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetWatchers();
        }
    }
}

