using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.Utils;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.Views;
using Octokit;
using Splat;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Repositories
{
    public class RepositoriesViewController : ListViewController<Repository>
	{
        public static RepositoriesViewController CreateTeamViewController(int id)
            => FromGitHub(ApiUrls.TeamRepositories(id));

        public static RepositoriesViewController CreateMineViewController()
            => FromGitHub(ApiUrls.Repositories(), showOwner: false, affiliation: "owner,collaborator");

        public static RepositoriesViewController CreateUserViewController(string username)
        {
            var applicationService = Locator.Current.GetService<IApplicationService>();
            var isCurrent = string.Equals(applicationService.Account.Username, username, StringComparison.OrdinalIgnoreCase);

            return isCurrent
                ? CreateMineViewController()
                : FromGitHub(ApiUrls.Repositories(username));
        }

        public static RepositoriesViewController CreateStarredViewController()
            => FromGitHub(ApiUrls.Starred(), "Starred");

        public static RepositoriesViewController CreateWatchedViewController()
            => FromGitHub(ApiUrls.Watched(), "Watched");

        public static RepositoriesViewController CreateForkedViewController(string username, string repository)
            => FromGitHub(ApiUrls.RepositoryForks(username, repository), "Forks");

        public static RepositoriesViewController CreateOrganizationViewController(string org)
            => FromGitHub(ApiUrls.OrganizationRepositories(org));

        private static RepositoriesViewController FromGitHub(
            Uri uri,
            string title = null,
            bool showOwner = true,
            bool showSearchBar = true,
            string affiliation = null,
            IApplicationService applicationService = null)
        {
            var parameters = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(affiliation))
                parameters["affiliation"] = affiliation;

            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            var dataRetriever = new GitHubList<Repository>(applicationService.GitHubClient, uri, parameters);

            return new RepositoriesViewController(dataRetriever)
            {
                Title = title ?? "Repositories",
                ShowSearchBar = showSearchBar,
                ShowOwner = showOwner
            };
        }

        private static EmptyListView CreateEmptyListView()
            => new EmptyListView(Octicon.Repo.ToEmptyListImage(), "There are no repositories!");

        public bool ShowOwner { get; set; } = true;

        public RepositoriesViewController(IDataRetriever<Repository> dataRetriever)
            : base(dataRetriever, CreateEmptyListView)
        {
        }

        protected override Element ConvertToElement(Repository item)
        {
            var e = new RepositoryElement(item, ShowOwner);

            e.Clicked
             .Select(_ => new RepositoryViewController(item))
             .Subscribe(this.PushViewController);

            return e;
        }
    }
}

