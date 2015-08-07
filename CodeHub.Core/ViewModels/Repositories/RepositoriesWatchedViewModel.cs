using CodeHub.Core.Services;
using System;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesWatchedViewModel : BaseRepositoriesViewModel
    {
        public RepositoriesWatchedViewModel(ISessionService applicationService) 
            : base(applicationService)
        {
            Title = "Watched";
        }

        protected override Uri RepositoryUri
        {
            get { return Octokit.ApiUrls.Watched(); }
        }
    }
}

