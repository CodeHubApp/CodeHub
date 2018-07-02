using System;
using System.Reactive.Linq;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.Views;
using CoreGraphics;
using Octokit;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Users
{
    public class UsersViewController : GitHubListViewController<User>
    {
        private readonly UISearchBar _repositorySearchBar = new UISearchBar(new CGRect(0, 0, 320, 44));
        private readonly LoadingIndicatorView _loading = new LoadingIndicatorView();

        public static UsersViewController CreateWatchersViewController(string owner, string name)
            => new UsersViewController(ApiUrls.Watchers(owner, name)) { Title = "Watchers" };

        public static UsersViewController CreateFollowersViewController(string username)
            => new UsersViewController(ApiUrls.Followers(username)) { Title = "Followers" };

        public static UsersViewController CreateFollowingViewController(string username)
            => new UsersViewController(ApiUrls.Following(username)) { Title = "Following" };

        public static UsersViewController CreateOrganizationMembersViewController(string org)
            => new UsersViewController(ApiUrls.Members(org)) { Title = "Members" };

        public static UsersViewController CreateStargazersViewController(string username, string repository)
            => new UsersViewController(ApiUrls.Stargazers(username, repository)) { Title = "Stargazers" };

        public static UsersViewController CreateTeamMembersViewController(int id)
            => new UsersViewController(ApiUrls.TeamMembers(id)) { Title = "Members" };

        public static UsersViewController CreateCollaboratorsViewController(string username, string repository)
            => new UsersViewController(ApiUrls.RepoCollaborators(username, repository)) { Title = "Collaborators" };

        private UsersViewController(Uri uri)
            : base(uri, Octicon.Person)
        {
            Title = "Users";
        }

        protected override Element ConvertToElement(User item)
        {
            var element = new UserElement(
                item.Login, 
                item.Name, 
                new Core.Utilities.GitHubAvatar(item.AvatarUrl));
            
            element
                .Clicked
                .Select(_ => new UserViewController(item))
                .Subscribe(this.PushViewController);

            return element;
        }
    }
}
