using System.Reactive;
using ReactiveUI;

namespace CodeHub.Core.ViewModels
{
    public interface ICanGoToViewModel
    {
        ReactiveCommand<Unit, Unit> GoToCommand { get; }
    }
}

