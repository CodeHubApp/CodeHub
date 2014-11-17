using ReactiveUI;
using System;

namespace CodeHub.Core.ViewModels.Source
{
    public class TagItemViewModel : ReactiveObject
    {
        public string Name { get; private set; }

        public IReactiveCommand<object> GoToCommand { get; private set; }

        public TagItemViewModel(string name, Action gotoCommand)
        {
            Name = name;
            GoToCommand = ReactiveCommand.Create().WithSubscription(_ => gotoCommand());
        }
    }
}

