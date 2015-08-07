using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Users;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryCollaboratorsViewModel : BaseUsersViewModel
    {
        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

        public RepositoryCollaboratorsViewModel(ISessionService sessionService)
            : base(sessionService)
        {
            Title = "Collaborators";
        }

        protected override System.Uri RequestUri
        {
            get { return Octokit.ApiUrls.RepoCollaborators(RepositoryOwner, RepositoryName); }
        }

        public RepositoryCollaboratorsViewModel Init(string repositoryOwner, string repositoryName)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            return this;
        }
    }
}

