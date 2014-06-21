using System.Collections.Generic;
using CodeHub.Core.Services;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Gists
{
    public class StarredGistsViewModel : GistsViewModel
    {
        private readonly IApplicationService _applicationService;

        public StarredGistsViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        protected override GitHubRequest<List<GistModel>> CreateRequest()
        {
			return _applicationService.Client.Gists.GetStarredGists();
        }
    }
}
