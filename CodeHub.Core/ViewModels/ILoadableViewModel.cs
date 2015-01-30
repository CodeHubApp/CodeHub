using ReactiveUI;
using System.Reactive;

namespace CodeHub.Core.ViewModels
{
    public interface ILoadableViewModel
    {
        IReactiveCommand<Unit> LoadCommand { get; }
    }
}

