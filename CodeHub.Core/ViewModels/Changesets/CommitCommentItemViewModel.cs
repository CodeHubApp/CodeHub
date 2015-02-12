using System;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class CommitCommentItemViewModel
    {
        public string Actor { get; private set; }

        public GitHubAvatar Avatar { get; private set; }

        public DateTime UtcCreatedAt { get; private set; }

        public string Body { get; private set; }

        internal CommitCommentItemViewModel(Octokit.CommitComment comment)
        {
            Avatar = new GitHubAvatar(comment.With(y => y.User).With(y => y.AvatarUrl));
            Actor = comment.With(x => x.User).With(x => x.Login);
            Body = comment.Body;
            UtcCreatedAt = comment.CreatedAt.UtcDateTime;
        }
    }
}

