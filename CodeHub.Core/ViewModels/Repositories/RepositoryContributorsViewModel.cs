using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Users;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryContributorsViewModel : BaseUsersViewModel
    {
        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set;  }

        public RepositoryContributorsViewModel(ISessionService sessionService)
            : base(sessionService)
        {
            Title = "Contributors";
        }

        protected override System.Uri RequestUri
        {
            get { return Octokit.ApiUrls.RepositoryContributors(RepositoryOwner, RepositoryName); }
        }

        public RepositoryContributorsViewModel Init(string repositoryOwner, string repositoryName)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            return this;
        }
    }
}

