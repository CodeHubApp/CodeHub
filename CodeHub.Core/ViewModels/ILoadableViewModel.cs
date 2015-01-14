using ReactiveUI;
using System.Reactive;

namespace CodeHub.Core.ViewModels
{
    public interface ILoadableViewModel : IBaseViewModel
    {
        IReactiveCommand<Unit> LoadCommand { get; }
    }
}

