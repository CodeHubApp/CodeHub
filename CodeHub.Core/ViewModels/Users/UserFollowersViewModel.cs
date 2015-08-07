using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserFollowersViewModel : BaseUsersViewModel
    {
        public string Username { get; private set; }

        public UserFollowersViewModel(ISessionService sessionService)
            : base(sessionService)
        {
            Title = "Followers";
        }

        protected override System.Uri RequestUri
        {
            get { return Octokit.ApiUrls.Followers(Username); }
        }

        public UserFollowersViewModel Init(string username)
        {
            Username = username;
            return this;
        }
    }
}

