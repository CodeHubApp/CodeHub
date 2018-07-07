using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.Utils;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.Views;
using CoreGraphics;
using Octokit;
using Splat;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Users
{
    public class UsersViewController : ListViewController<User>
    {
        private readonly UISearchBar _repositorySearchBar = new UISearchBar(new CGRect(0, 0, 320, 44));
        private readonly LoadingIndicatorView _loading = new LoadingIndicatorView();

        public static UsersViewController CreateWatchersViewController(string owner, string name)
        {
            var viewCtrl = FromGitHub(ApiUrls.Watchers(owner, name));
            viewCtrl.Title = "Watchers";
            return viewCtrl;
        }

        public static UsersViewController CreateFollowersViewController(string username)
        {
            var viewCtrl = FromGitHub(ApiUrls.Followers(username));
            viewCtrl.Title = "Followers";
            return viewCtrl;
        }

        public static UsersViewController CreateFollowingViewController(string username)
        {
            var viewCtrl = FromGitHub(ApiUrls.Following(username));
            viewCtrl.Title = "Following";
            return viewCtrl;
        }

        public static UsersViewController CreateOrganizationMembersViewController(string org)
        {
            var viewCtrl = FromGitHub(ApiUrls.Members(org));
            viewCtrl.Title = "Members";
            return viewCtrl;
        }

        public static UsersViewController CreateStargazersViewController(string username, string repository)
        {
            var viewCtrl = FromGitHub(ApiUrls.Stargazers(username, repository));
            viewCtrl.Title = "Stargazers";
            return viewCtrl;
        }

        public static UsersViewController CreateTeamMembersViewController(int id)
        {
            var viewCtrl = FromGitHub(ApiUrls.TeamMembers(id));
            viewCtrl.Title = "Members";
            return viewCtrl;
        }

        public static UsersViewController CreateCollaboratorsViewController(string username, string repository)
        {
            var viewCtrl = FromGitHub(ApiUrls.RepoCollaborators(username, repository));
            viewCtrl.Title = "Collaborators";
            return viewCtrl;
        }

        public static UsersViewController FromGitHub(
            Uri uri,
            IApplicationService applicationService = null)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            var dataRetriever = new GitHubList<User>(applicationService.GitHubClient, uri);
            return new UsersViewController(dataRetriever);
        }

        private static EmptyListView CreateEmptyListView()
            => new EmptyListView(Octicon.Person.ToEmptyListImage(), "There are no users!");

        private UsersViewController(IDataRetriever<User> dataRetriever)
            : base(dataRetriever, CreateEmptyListView)
        {
            Title = "Users";
        }

        protected override Element ConvertToElement(User item)
        {
            var element = new ProfileElement(
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
