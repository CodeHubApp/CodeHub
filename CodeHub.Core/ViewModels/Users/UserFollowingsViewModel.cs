using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserFollowingsViewModel : BaseUserCollectionViewModel
    {
        private readonly IApplicationService _applicationService;

        public string Username { get; set; }

        public UserFollowingsViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;
            Title = "Following";
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.BasicUserModel>> CreateRequest()
        {
            return _applicationService.Client.Users[Username].GetFollowing();
        }
    }
}