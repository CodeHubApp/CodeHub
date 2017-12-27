using ReactiveUI;
using System;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class CommitItemViewModel : ReactiveObject
    {
        public string Title { get; }

        public string Name { get; }

        public GitHubAvatar Avatar { get; }

        public DateTimeOffset? Date { get; }

        public Octokit.GitHubCommit Commit { get; }

        public CommitItemViewModel(Octokit.GitHubCommit commit)
        {
            Avatar = new GitHubAvatar(commit.GenerateGravatarUrl());
            Name = commit.GenerateCommiterName();
            Commit = commit;

            var msg = commit.Commit?.Message ?? string.Empty;
            var firstLine = msg.IndexOf("\n", StringComparison.Ordinal);
            Title = firstLine > 0 ? msg.Substring(0, firstLine) : msg;
            Date = commit.Commit?.Committer?.Date ?? commit.Commit?.Author.Date;
        }
    }
}

