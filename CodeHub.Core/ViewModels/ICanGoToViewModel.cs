using ReactiveUI;

namespace CodeHub.Core.ViewModels
{
    public interface ICanGoToViewModel
    {
        IReactiveCommand<object> GoToCommand { get; }
    }
}

