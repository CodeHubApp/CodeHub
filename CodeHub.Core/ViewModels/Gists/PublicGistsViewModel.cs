using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Gists
{
    public class PublicGistsViewModel : BaseGistsViewModel
    {
        private readonly IApplicationService _applicationService;

        public PublicGistsViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;
            Title = "Public Gists";
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.GistModel>> CreateRequest()
        {
            return _applicationService.Client.Gists.GetPublicGists();
        }
    }
}
