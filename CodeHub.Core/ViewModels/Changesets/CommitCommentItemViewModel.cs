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

        internal CommitCommentItemViewModel(GitHubSharp.Models.CommentModel comment)
        {
            Avatar = new GitHubAvatar(comment?.User?.AvatarUrl);
            Actor = comment?.User?.Login;
            Body = comment.BodyHtml;
            UtcCreatedAt = comment.CreatedAt.UtcDateTime;
        }
    }
}

