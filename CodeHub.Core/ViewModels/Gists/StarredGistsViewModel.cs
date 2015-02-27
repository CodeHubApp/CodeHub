using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Gists
{
    public class StarredGistsViewModel : BaseGistsViewModel
    {
        private readonly ISessionService _applicationService;

        public StarredGistsViewModel(ISessionService applicationService)
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
