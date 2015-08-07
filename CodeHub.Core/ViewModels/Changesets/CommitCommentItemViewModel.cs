using System;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class CommitCommentItemViewModel
    {
        public string Actor { get; }

        public GitHubAvatar Avatar { get; }

        public DateTime UtcCreatedAt { get; }

        public string Body { get; }

        internal CommitCommentItemViewModel(GitHubSharp.Models.CommentModel comment)
        {
            Avatar = new GitHubAvatar(comment?.User?.AvatarUrl);
            Actor = comment?.User?.Login;
            Body = comment.BodyHtml;
            UtcCreatedAt = comment.CreatedAt.UtcDateTime;
        }
    }
}

