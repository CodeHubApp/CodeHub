using System.Collections.Generic;
using CodeHub.Core.Services;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Gists
{
    public class PublicGistsViewModel : GistsViewModel
    {
        private readonly IApplicationService _applicationService;

        public PublicGistsViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        protected override GitHubRequest<List<GistModel>> CreateRequest()
        {
            return _applicationService.Client.Gists.GetPublicGists();
        }
    }
}
