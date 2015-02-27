using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Gists
{
    public class PublicGistsViewModel : BaseGistsViewModel
    {
        private readonly ISessionService _applicationService;

        public PublicGistsViewModel(ISessionService applicationService)
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
