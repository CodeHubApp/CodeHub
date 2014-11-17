using ReactiveUI;
using System;

namespace CodeHub.Core.ViewModels.Source
{
    public class BranchItemViewModel : ReactiveObject
    {
        public string Name { get; private set; }

        public IReactiveCommand<object> GoToCommand { get; private set; }

        public BranchItemViewModel(string name, Action gotoCommand)
        {
            Name = name;
            GoToCommand = ReactiveCommand.Create().WithSubscription(_ => gotoCommand());
        }
    }
}

