using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using CodeHub.iOS.DialogElements;
using Octokit;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Issues
{
    public class IssueListViewController : GitHubListViewController<Issue>
    {
        public static IssueListViewController MyIssues(IDictionary<string, string> parameters = null)
            => new IssueListViewController(ApiUrls.Issues(), parameters);

        public static IssueListViewController RepositoryIssues(
            string username,
            string repository,
            IDictionary<string, string> parameters = null)
            => new IssueListViewController(ApiUrls.Issues(username, repository), parameters);

        private IssueListViewController(Uri uri, IDictionary<string, string> parameters)
            : base(uri, Octicon.IssueOpened, parameters: parameters)
        {
        }

        protected override Element ConvertToElement(Issue item)
        {
            var element = new IssueElement(item);
            element.Clicked.Select(_ => new IssueViewController(item)).Subscribe(this.PushViewController);
            return element;
        }
    }
}
