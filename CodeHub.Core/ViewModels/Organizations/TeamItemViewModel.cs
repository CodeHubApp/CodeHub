using ReactiveUI;
using System;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class TeamItemViewModel : ReactiveObject
    {
        public string Name { get; private set; }

        public IReactiveCommand GoToCommand { get; private set; }

        public TeamItemViewModel(string name, Action gotoCommand)
        {
            Name = name;
            GoToCommand = ReactiveCommand.Create().WithSubscription(x => gotoCommand());
        }
    }
}

