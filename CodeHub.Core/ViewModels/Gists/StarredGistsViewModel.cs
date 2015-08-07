using CodeHub.Core.Services;
using System;

namespace CodeHub.Core.ViewModels.Gists
{
    public class StarredGistsViewModel : BaseGistsViewModel
    {
        public StarredGistsViewModel(ISessionService sessionService)
            : base(sessionService)
        {
            Title = "Starred Gists";
        }

        protected override Uri RequestUri
        {
            get { return Octokit.ApiUrls.StarredGists(); }
        }
    }
}
