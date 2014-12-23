using System;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Issues
{
    public interface IIssueEventItemViewModel
    {
        DateTimeOffset CreatedAt { get; }

        string Actor { get; }

        string AvatarUrl { get; }
    }

    public class IssueEventItemViewModel : ReactiveObject, IIssueEventItemViewModel
    {
        public string Actor { get; private set; }

        public string AvatarUrl { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public string Commit { get; private set; }

        public Octokit.EventInfo EventInfo { get; private set; }

        internal IssueEventItemViewModel(Octokit.EventInfo issueEvent)
        {
            Actor = issueEvent.Actor.Name;
            AvatarUrl = issueEvent.Actor.AvatarUrl;
            CreatedAt = issueEvent.CreatedAt;
            Commit = issueEvent.CommitId;
            EventInfo = issueEvent;
        }
    }

    public class IssueCommentItemViewModel : ReactiveObject, IIssueEventItemViewModel
    {
        public string Comment { get; private set; }

        public string Actor { get; private set; }

        public string AvatarUrl { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        internal IssueCommentItemViewModel(Octokit.IssueComment comment)
        {
            Comment = comment.Body;
            Actor = comment.User.Name;
            AvatarUrl = comment.User.AvatarUrl;
            CreatedAt = comment.CreatedAt;
        }
    }
}

