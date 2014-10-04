using System;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Releases
{
    public class ReleaseItemViewModel : ReactiveObject
    {
        public string Name { get; private set; }

        public string Created { get; private set; }

        public IReactiveCommand GoToCommand { get; private set; }

        internal ReleaseItemViewModel(string name, DateTimeOffset createdAt,
            Action<ReleaseItemViewModel> gotoCommand)
        {
            Name = name;
            Created = createdAt.LocalDateTime.ToShortDateString();
            GoToCommand = ReactiveCommand.Create().WithSubscription(x => gotoCommand(this));
        }
    }
}

