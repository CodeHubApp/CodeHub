using ReactiveUI;
using System.Reactive;

namespace CodeHub.Core.ViewModels
{
    public interface ILoadableViewModel
    {
        ReactiveCommand<Unit, Unit> LoadCommand { get; }
    }
}

