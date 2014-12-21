using System;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Releases
{
    public class ReleaseItemViewModel : ReactiveObject
    {
        public string Name { get; private set; }

        public DateTime Created { get; private set; }

        public long Id { get; private set; }

        public IReactiveCommand GoToCommand { get; private set; }

        internal ReleaseItemViewModel(long id, string name, DateTimeOffset createdAt,
            Action<ReleaseItemViewModel> gotoCommand)
        {
            Id = id;
            Name = name;
            Created = createdAt.LocalDateTime;
            GoToCommand = ReactiveCommand.Create().WithSubscription(x => gotoCommand(this));
        }
    }
}

