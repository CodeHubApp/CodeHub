using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserFollowingsViewModel : BaseUsersViewModel
    {
        public string Username { get; private set; }

        public UserFollowingsViewModel(ISessionService sessionService)
            : base(sessionService)
        {
            Title = "Following";
        }

        protected override System.Uri RequestUri
        {
            get { return Octokit.ApiUrls.Following(Username); }
        }

        public UserFollowingsViewModel Init(string username)
        {
            Username = username;
            return this;
        }
    }
}