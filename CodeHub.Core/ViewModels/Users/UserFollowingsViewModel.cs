using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserFollowingsViewModel : BaseUsersViewModel
    {
        private readonly ISessionService _applicationService;

        public string Username { get; private set; }

        public UserFollowingsViewModel(ISessionService applicationService)
        {
            _applicationService = applicationService;
            Title = "Following";
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<GitHubSharp.Models.BasicUserModel>> CreateRequest()
        {
            return _applicationService.Client.Users[Username].GetFollowing();
        }

        public UserFollowingsViewModel Init(string username)
        {
            Username = username;
            return this;
        }
    }
}