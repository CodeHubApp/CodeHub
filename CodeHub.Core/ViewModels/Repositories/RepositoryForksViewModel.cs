using CodeHub.Core.Services;
using System;
using Octokit;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryForksViewModel : BaseRepositoriesViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public RepositoryForksViewModel(ISessionService applicationService)
            : base(applicationService)
        {
            Title = "Forks";
        }

        protected override Uri RepositoryUri
        {
            get { return ApiUrls.RepositoryForks(RepositoryOwner, RepositoryName); }
        }
    }
}
