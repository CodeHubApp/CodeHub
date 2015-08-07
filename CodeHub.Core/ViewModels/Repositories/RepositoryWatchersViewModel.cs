using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Users;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryWatchersViewModel : BaseUsersViewModel
    {
        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

        public RepositoryWatchersViewModel(ISessionService sessionService)
            : base(sessionService)
	    {
            Title = "Watchers";
	    }

        protected override System.Uri RequestUri
        {
            get { return Octokit.ApiUrls.Watchers(RepositoryOwner, RepositoryName); }
        }

        public RepositoryWatchersViewModel Init(string repositoryOwner, string repositoryName)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            return this;
        }
    }
}

