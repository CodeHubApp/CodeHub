using System;
using ReactiveUI;
using GitHubSharp.Models;
using CodeHub.Core.Utilities;
using Humanizer;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class CommitItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Name { get; private set; }

        public GitHubAvatar Avatar { get; private set; }

        public string Description { get; private set; }

        public string Time { get; private set; }

        public IReactiveCommand<object> GoToCommand { get; private set; }

        internal CommitModel Commit { get; private set; }

        internal CommitItemViewModel(CommitModel commit, Action<CommitItemViewModel> action)
        {
            var msg = commit.With(x => x.Commit).With(x => x.Message, () => string.Empty);
            var firstLine = msg.IndexOf("\n", StringComparison.Ordinal);
            Description = firstLine > 0 ? msg.Substring(0, firstLine) : msg;

            var time = DateTimeOffset.MinValue;
            if (commit.Commit.Committer != null)
                time = commit.Commit.Committer.Date;
            Time = time.UtcDateTime.Humanize();

            Name = commit.GenerateCommiterName();
            Avatar = new GitHubAvatar(commit.GenerateGravatarUrl());
            Commit = commit;
            GoToCommand = ReactiveCommand.Create().WithSubscription(_ => action(this));
        }
    }
}

