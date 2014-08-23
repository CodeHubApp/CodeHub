using ReactiveUI;
using System;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryItemViewModel : ReactiveObject
    {
        public string Name { get; private set; }

        public string Owner { get; private set; }

        public string ImageUrl { get; private set; }

        public string Description { get; private set; }

        public int Stars { get; private set; }

        public int Forks { get; private set; }

        public IReactiveCommand GoToCommand { get; private set; }

        internal RepositoryItemViewModel(string name, string owner, string imageUrl,
                                         string description, int stars, int forks,
                                         Action<RepositoryItemViewModel> gotoCommand)
        {
            Name = name;
            Owner = owner;
            ImageUrl = imageUrl;
            Description = description;
            Stars = stars;
            Forks = forks;
            GoToCommand = ReactiveCommand.Create().WithSubscription(x => gotoCommand(this));
        }
    }
}

