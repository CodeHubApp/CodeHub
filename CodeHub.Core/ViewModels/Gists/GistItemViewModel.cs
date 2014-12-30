using System;
using ReactiveUI;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistItemViewModel : ReactiveObject
    {
        public GitHubAvatar Avatar { get; private set; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public DateTimeOffset UpdatedAt { get; private set; }

        public IReactiveCommand GoToCommand { get; private set; }

        public GistItemViewModel(string title, string avatarUrl, string description, DateTimeOffset updatedAt, Action<GistItemViewModel> gotoAction)
        {
            Title = title;
            Description = description;
            UpdatedAt = updatedAt;
            GoToCommand = ReactiveCommand.Create().WithSubscription(x => gotoAction(this));
            Avatar = new GitHubAvatar(avatarUrl);
        }
    }
}

