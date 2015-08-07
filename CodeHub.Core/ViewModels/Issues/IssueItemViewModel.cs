using ReactiveUI;
using CodeHub.Core.Utilities;
using System;
using Octokit;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public int Number { get; private set; }

        public string RepositoryName { get; private set; }

        public string RepositoryFullName { get; private set; }

        public string RepositoryOwner { get; private set; }

        public bool IsPullRequest { get; private set; }

        public string Title { get; private set; }

        public string State { get; private set; }

        public int Comments { get; private set; }

        public string Assignee { get; private set; }

        public DateTimeOffset UpdatedAt { get; private set; }

        public IReactiveCommand<object> GoToCommand { get; private set; }

        internal IssueItemViewModel(Issue issue)
        {
            var isPullRequest = issue.PullRequest != null && issue.PullRequest.HtmlUrl != null;
            var s1 = issue.Url.AbsolutePath.Substring(issue.Url.AbsolutePath.IndexOf("/repos/", StringComparison.Ordinal) + 7);
            var repoId = new RepositoryIdentifier(s1.Substring(0, s1.IndexOf("/issues", StringComparison.Ordinal)));

            RepositoryFullName = repoId.Owner + "/" + repoId.Name;
            RepositoryName = repoId.Name;
            RepositoryOwner = repoId.Owner;
            IsPullRequest = isPullRequest;
            Title = issue.Title;
            Number = issue.Number;
            State = issue.State.ToString();
            Comments = issue.Comments;
            Assignee = issue.Assignee != null ? issue.Assignee.Login : "Unassigned";
            UpdatedAt = issue.UpdatedAt ?? DateTimeOffset.Now;
            GoToCommand = ReactiveCommand.Create();
        }
    }
}

