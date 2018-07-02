using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using CodeHub.iOS.DialogElements;
using Octokit;
using UIKit;

namespace CodeHub.iOS.ViewControllers.PullRequests
{
    public class PullRequestListViewController : GitHubListViewController<PullRequest>
    {
        private readonly string _username;
        private readonly string _repository;

        public PullRequestListViewController(string username, string repository, ItemState state)
            : base(ApiUrls.PullRequests(username, repository), Octicon.GitPullRequest,
                   parameters: new Dictionary<string, string> { { "state", state.ToString().ToLower() } })
        {
            _username = username;
            _repository = repository;
        }

        protected override Element ConvertToElement(PullRequest item)
        {
            var element = new PullRequestElement(item);
            element
                .Clicked
                .Select(_ => new PullRequestViewController(_username, _repository, item))
                .Subscribe(this.PushViewController);
            return element;
        }
    }
}
