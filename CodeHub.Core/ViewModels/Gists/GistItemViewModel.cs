using System;
using ReactiveUI;
using CodeHub.Core.Utilities;
using Humanizer;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public GitHubAvatar Avatar { get; }

        public string Title { get; }

        public string Description { get; }

        public DateTimeOffset UpdatedAt { get; }

        public string UpdatedString { get; }

        public IReactiveCommand<object> GoToCommand { get; }

        public GistItemViewModel(string title, string avatarUrl, string description, DateTimeOffset updatedAt, Action<GistItemViewModel> gotoAction)
        {
            Title = title;
            Description = description;
            UpdatedAt = updatedAt;
            UpdatedString = UpdatedAt.Humanize();
            GoToCommand = ReactiveCommand.Create().WithSubscription(x => gotoAction(this));
            Avatar = new GitHubAvatar(avatarUrl);
        }
    }
}

