using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace CodeHub.Core.ViewModels
{
    public class ActionMenuViewModel
    {
        public IReadOnlyList<ActionMenuItem> Items { get; private set; }

        public ActionMenuViewModel(IEnumerable<ActionMenuItem> items)
        {
            Items = new ReadOnlyCollection<ActionMenuItem>(items.ToList());
        }
    }

    public class ActionMenuItem
    {
        public string Name { get; private set; }

        public IReactiveCommand Command { get; private set; }

        public ActionMenuItem(string name, IReactiveCommand command)
        {
            Name = name;
            Command = command;
        }
    }
}

