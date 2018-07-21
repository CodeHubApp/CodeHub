using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using CodeHub.iOS.DialogElements;
using Octokit;
using UIKit;
using Splat;
using CodeHub.Core.Services;
using CodeHub.Core.Utils;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.PullRequests
{
    public class PullRequestListViewController : ListViewController<PullRequest>
    {
        private static EmptyListView CreateEmptyListView()
            => new EmptyListView(Octicon.GitPullRequest.ToEmptyListImage(), "There are no pull-requests!");

        public static PullRequestListViewController FromGitHub(
            string username,
            string repository,
            ItemState state,
            IApplicationService applicationService = null)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            var parameters = new Dictionary<string, string> { { "state", state.ToString().ToLower() } };
            var uri = ApiUrls.PullRequests(username, repository);
            var dataRetriever = new GitHubList<PullRequest>(applicationService.GitHubClient, uri);
            return new PullRequestListViewController(dataRetriever);
        }

        protected PullRequestListViewController(IDataRetriever<PullRequest> dataRetriever)
            : base(dataRetriever, CreateEmptyListView)
        {
        }

        protected override Element ConvertToElement(PullRequest item)
        {
            var element = new PullRequestElement(item);
            element
                .Clicked
                .Select(_ => new PullRequestViewController(item))
                .Subscribe(this.PushViewController);
            return element;
        }
    }
}
