using CodeHub.Core.Services;
using System;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesStarredViewModel : BaseRepositoriesViewModel
    {
        public RepositoriesStarredViewModel(ISessionService applicationService) 
            : base(applicationService)
        {
            Title = "Starred";
        }

        protected override Uri RepositoryUri
        {
            get { return Octokit.ApiUrls.Starred(); }
        }
    }
}

