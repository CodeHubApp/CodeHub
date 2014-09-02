using System;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class CommitItemViewModel : ReactiveObject
    {
        public string Name { get; private set; }

        public string ImageUrl { get; private set; }

        public string Description { get; private set; }

        public DateTimeOffset Time { get; private set; }

        public IReactiveCommand GoToCommand { get; private set; }

        internal CommitItemViewModel(string name, string imageUrl, string description, DateTimeOffset time, Action<CommitItemViewModel> action)
        {
            Name = name;
            ImageUrl = imageUrl;
            Description = description;
            Time = time;
            GoToCommand = ReactiveCommand.Create().WithSubscription(_ => action(this));
        }
    }
}

