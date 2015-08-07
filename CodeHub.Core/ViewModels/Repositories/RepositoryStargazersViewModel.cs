using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Users;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryStargazersViewModel : BaseUsersViewModel
    {
        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

	    public RepositoryStargazersViewModel(ISessionService sessionService)
            : base(sessionService)
	    {
            Title = "Stargazers";
	    }

        protected override System.Uri RequestUri
        {
            get { return Octokit.ApiUrls.Stargazers(RepositoryOwner, RepositoryName); }
        }

        public RepositoryStargazersViewModel Init(string repositoryOwner, string repositoryName)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            return this;
        }
    }
}

