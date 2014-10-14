using System;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistItemViewModel : ReactiveObject
    {
        public string ImageUrl { get; private set; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public DateTimeOffset UpdatedAt { get; private set; }

        public IReactiveCommand GoToCommand { get; private set; }

        public GistItemViewModel(string title, string imageUrl, string description, DateTimeOffset updatedAt, Action<GistItemViewModel> gotoAction)
        {
            Title = title;
            ImageUrl = imageUrl;
            Description = description;
            UpdatedAt = updatedAt;
            GoToCommand = ReactiveCommand.Create().WithSubscription(x => gotoAction(this));
        }
    }
}

