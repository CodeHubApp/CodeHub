using System;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistItemViewModel : ReactiveObject
    {
        public string Owner { get; private set; }

        public string ImageUrl { get; private set; }

        public string Description { get; private set; }

        public DateTimeOffset UpdatedAt { get; private set; }

        public IReactiveCommand GoToCommand { get; private set; }

        public GistItemViewModel(string owner, string imageUrl, string description, DateTimeOffset updatedAt, Action<GistItemViewModel> gotoAction)
        {
            Owner = owner;
            ImageUrl = imageUrl;
            Description = description;
            UpdatedAt = updatedAt;
            GoToCommand = ReactiveCommand.Create().WithSubscription(x => gotoAction(this));
        }
    }
}

