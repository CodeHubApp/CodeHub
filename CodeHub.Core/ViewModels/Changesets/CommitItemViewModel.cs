using System;
using ReactiveUI;
using GitHubSharp.Models;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class CommitItemViewModel : ReactiveObject
    {
        public string Name { get; private set; }

        public GitHubAvatar Avatar { get; private set; }

        public string Description { get; private set; }

        public DateTimeOffset Time { get; private set; }

        public IReactiveCommand GoToCommand { get; private set; }

        internal CommitModel Commit { get; private set; }

        internal CommitItemViewModel(CommitModel commit, Action<CommitItemViewModel> action)
        {
            var msg = commit.Commit.Message ?? string.Empty;
            var firstLine = msg.IndexOf("\n", StringComparison.Ordinal);
            Description = firstLine > 0 ? msg.Substring(0, firstLine) : msg;

            Time = DateTimeOffset.MinValue;
            if (commit.Commit.Committer != null)
                Time = commit.Commit.Committer.Date;

            Name = commit.GenerateCommiterName();
            Avatar = new GitHubAvatar(commit.GenerateGravatarUrl());
            Commit = commit;
            GoToCommand = ReactiveCommand.Create().WithSubscription(_ => action(this));
        }
    }
}

