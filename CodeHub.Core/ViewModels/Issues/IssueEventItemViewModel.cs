using System;
using ReactiveUI;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.ViewModels.Issues
{
    public interface IIssueEventItemViewModel
    {
        DateTimeOffset CreatedAt { get; }

        string Actor { get; }

        GitHubAvatar AvatarUrl { get; }
    }

    public class IssueEventItemViewModel : ReactiveObject, IIssueEventItemViewModel
    {
        public string Actor { get; private set; }

        public GitHubAvatar AvatarUrl { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public string Commit { get; private set; }

        public Octokit.EventInfo EventInfo { get; private set; }

        internal IssueEventItemViewModel(Octokit.EventInfo issueEvent)
        {
            Actor = issueEvent.With(x => x.Actor).With(x => x.Login, () => "Deleted User");
            AvatarUrl = new GitHubAvatar(issueEvent.With(x => x.Actor).With(x => x.AvatarUrl));
            CreatedAt = issueEvent.CreatedAt;
            Commit = issueEvent.CommitId;
            EventInfo = issueEvent;
        }
    }

    public class IssueCommentItemViewModel : ReactiveObject, IIssueEventItemViewModel
    {
        public string Comment { get; private set; }

        public string Actor { get; private set; }

        public GitHubAvatar AvatarUrl { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        internal IssueCommentItemViewModel(GitHubSharp.Models.IssueCommentModel comment)
        {
            Comment = comment.BodyHtml;
            Actor = comment.With(x => x.User).With(x => x.Login);
            AvatarUrl = new GitHubAvatar(comment.With(x => x.User).With(x => x.AvatarUrl));
            CreatedAt = comment.CreatedAt;
        }
    }
}

