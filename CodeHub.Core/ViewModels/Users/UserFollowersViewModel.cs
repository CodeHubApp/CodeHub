using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserFollowersViewModel : BaseUserCollectionViewModel
    {
        private readonly IApplicationService _applicationService;

        public string Username { get; set; }

        public UserFollowersViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;
            Title = "Followers";
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.BasicUserModel>> CreateRequest()
        {
            return _applicationService.Client.Users[Username].GetFollowers();
        }
    }
}

