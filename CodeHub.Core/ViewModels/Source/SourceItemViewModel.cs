using System;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Source
{
    public class SourceItemViewModel : ReactiveObject
    {
        public string Name { get; private set; }

        public SourceItemType Type { get; private set; }

        public IReactiveCommand GoToCommand { get; private set; }

        internal SourceItemViewModel(string name, SourceItemType type, Action<SourceItemViewModel> gotoAction)
        {
            Name = name;
            Type = type;
            GoToCommand = ReactiveCommand.Create().WithSubscription(x => gotoAction(this));
        }
    }

    public enum SourceItemType
    {
        File = 0,
        Directory,
        Submodule
    }
}

