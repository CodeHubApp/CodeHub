using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.Utils;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.Views;
using Octokit;
using Splat;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Users
{
    public class UsersViewController : ListViewController<User>
    {
        public static UsersViewController CreateWatchersViewController(string owner, string name)
            => FromGitHub(ApiUrls.Watchers(owner, name), "Watchers");

        public static UsersViewController CreateFollowersViewController(string username)
            => FromGitHub(ApiUrls.Followers(username), "Followers");

        public static UsersViewController CreateFollowingViewController(string username)
            => FromGitHub(ApiUrls.Following(username), "Following");

        public static UsersViewController CreateOrganizationMembersViewController(string org)
            => FromGitHub(ApiUrls.Members(org), "Members");

        public static UsersViewController CreateStargazersViewController(string username, string repository)
            => FromGitHub(ApiUrls.Stargazers(username, repository), "Stargazers");

        public static UsersViewController CreateTeamMembersViewController(int id)
            => FromGitHub(ApiUrls.TeamMembers(id), "Members");

        public static UsersViewController CreateCollaboratorsViewController(string username, string repository)
            => FromGitHub(ApiUrls.RepoCollaborators(username, repository), "Collaborators");

        public static UsersViewController FromGitHub(
            Uri uri,
            string title,
            IApplicationService applicationService = null)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            var dataRetriever = new GitHubList<User>(applicationService.GitHubClient, uri);
            return new UsersViewController(dataRetriever) { Title = title };
        }

        private static EmptyListView CreateEmptyListView()
            => new EmptyListView(Octicon.Person.ToEmptyListImage(), "There are no users!");

        public UsersViewController(IDataRetriever<User> dataRetriever)
            : base(dataRetriever, CreateEmptyListView)
        {
        }

        protected override Element ConvertToElement(User item)
        {
            var element = ProfileElement.FromUser(item);
            
            element
                .Clicked
                .Select(_ => new UserViewController(item))
                .Subscribe(this.PushViewController);

            return element;
        }
    }
}
