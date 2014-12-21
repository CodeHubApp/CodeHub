using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Gists
{
    public class StarredGistsViewModel : BaseGistsViewModel
    {
        private readonly IApplicationService _applicationService;

        public StarredGistsViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;
            Title = "Starred Gists";
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.GistModel>> CreateRequest()
        {
            return _applicationService.Client.Gists.GetStarredGists();
        }
    }
}
